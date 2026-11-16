"""Convert the supplied OSM export to Unity's local metre grid; no downloaded elevation."""
import json, math, shutil
from pathlib import Path
def project(p):
    return ((p[0]-126.9200)*111320*math.cos(math.radians(37.3913)), (p[1]-37.3913)*111320)

SOURCE=Path(r'C:\Users\user\Downloads\export.geojson')
OUT=Path('Assets/Art/AnyangCampus/Geo');OUT.mkdir(parents=True,exist_ok=True)
raw=json.loads(SOURCE.read_text(encoding='utf-8-sig'))
shutil.copyfile(SOURCE,OUT/'Source.geojson')
known={
 '1323539448':('수리관','suri',8,32), '1323539449':('대신관','front',3,17),
 '1323539446':('수봉관','tower',23,48), '1323539447':('문화관','chapel',13,19),
 '1323539443':('일우 중앙도서관','library',49,27),
 '475598277':('아리관','leader',49,23), '475598276':('아리비전센터','vision',49,20),
 '475598279':('창의융합산학관','innovation',43,18),
 '475598280':('연결다리','bridge',61,4),
 '1323539442':('상부 연결통로','bridge',58,4),
 '1323539444':('상부 부속건물','auxiliary',46,5),
 '1323539450':('해오름재','brick',4,35), '475600870':('벤치 쉼터','roof',12,3)
}
anchors=[(53,99,0),(22,113,4),(3,86,11),(-3,53,18),(1,47,20),(34,19,24),(57,-15,29),(49,-27,30),
 (4,13,34),(-45,12,43),(-108,12,44),(-112,-56,49),(-119,-102,49),(95,35,7),(107,-3,12)]
def elevation(x,z):
    weights=[1/(max(3,math.hypot(x-a,z-b))**3) for a,b,h in anchors]
    return sum(w*a[2] for w,a in zip(weights,anchors))/sum(weights)
def xyz(p):
    x,z=project(p);return {'x':round(x,4),'y':round(elevation(x,z),4),'z':round(z,4)}
def cross(a,b,c):return (b['x']-a['x'])*(c['z']-a['z'])-(b['z']-a['z'])*(c['x']-a['x'])
def triangulate(points):
    area=sum(a['x']*b['z']-b['x']*a['z'] for a,b in zip(points,points[1:]+points[:1]))
    ids=list(range(len(points)));ids=ids if area>0 else list(reversed(ids));tris=[]
    def inside(a,b,c,p):return min(cross(a,b,p),cross(b,c,p),cross(c,a,p))>=-1e-7
    while len(ids)>3:
        for k,i in enumerate(ids):
            a,b,c=ids[k-1],i,ids[(k+1)%len(ids)]
            if cross(points[a],points[b],points[c])<=1e-7:continue
            if any(inside(points[a],points[b],points[c],points[j]) for j in ids if j not in (a,b,c)):continue
            tris.extend((a,c,b));ids.pop(k);break
        else:raise ValueError('Cannot triangulate footprint')
    tris.extend((ids[0],ids[2],ids[1]));return tris
buildings=[];roads=[]
for f in raw['features']:
    g,p=f['geometry'],f['properties'];ident=f['id'].split('/')[-1]
    if g['type'] in ('Polygon','MultiPolygon'):
        polygons=[g['coordinates']] if g['type']=='Polygon' else g['coordinates']
        for part,poly in enumerate(polygons):
            points=[xyz(q) for q in poly[0]][:-1]
            # OSM can contain collinear facade vertices; remove only numerically redundant points.
            changed=True
            while changed and len(points)>3:
                changed=False
                for k in range(len(points)):
                    if abs(cross(points[k-1],points[k],points[(k+1)%len(points)]))<1e-5:points.pop(k);changed=True;break
            x=sum(q['x'] for q in points)/len(points);z=sum(q['z'] for q in points)/len(points)
            name,kind,base,height=known.get(ident,(p.get('name',p.get('name:ko','주변 건물')),p.get('building','yes'),elevation(x,z),float(p.get('height',float(p.get('building:levels',4))*3.1))))
            buildings.append(dict(id=f['id']+(':'+str(part) if part else ''),name=name,kind=kind,campus=ident in known,baseY=round(base,3),height=height,points=points,triangles=triangulate(points)))
    elif g['type']=='LineString':
        pts=[xyz(q) for q in g['coordinates']]
        campus=ident in ('1323539452','1323539453','1323539454')
        # Explicit visual height anchors follow the existing photographed uphill road, not inferred GeoJSON altitudes.
        h=p.get('highway','');width=3.4 if campus else 6 if h in ('primary','secondary','tertiary') else 3.3 if h=='residential' else 2.5
        roads.append(dict(id=f['id'],name=p.get('name','교내 진입로' if campus else '주변 도로'),campus=campus,width=width,points=pts))
byid={r['id']:r['points'] for r in roads}
main=byid['way/1323539454']
def nearest(seq,p):return min(range(len(seq)),key=lambda i:(seq[i]['x']-p[0])**2+(seq[i]['z']-p[1])**2)
end=nearest(main,(-3,14))
left,right=main[end],main[end+1]
fraction=(-45-left['x'])/(right['x']-left['x'])
entry={'x':-45,'y':43.1,'z':round(left['z']+(right['z']-left['z'])*fraction,4)}
# A collinear node preserves the exported horizontal road while meeting the upper terrace.
main.insert(end+1,entry)
goal={'x':-45,'y':43.2,'z':4}
tail=[entry,goal]
path_a=main[:end+1]+tail
branch=list(reversed(byid['way/1323539452'][nearest(byid['way/1323539452'],(95,35)):]))
branch+=byid['way/1323539453'][1:]
path_b=branch+main[nearest(main,(3,86))+1:end+1]+tail
result=dict(originLon=126.9200,originLat=37.3913,metresPerUnit=1,attribution='© OpenStreetMap contributors, ODbL 1.0. Source: supplied overpass-turbo export.',
            buildings=buildings,roads=roads,anchors=[{'x':x,'y':h,'z':z} for x,z,h in anchors],
            paths=[dict(id='Campus_Gate',points=path_a),dict(id='Campus_Parking',points=path_b)])
(OUT/'CampusGeo.json').write_text(json.dumps(result,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
print(f'Prepared {len(buildings)} polygon parts from 81 buildings; {len(roads)} roads; {sum(b["campus"] for b in buildings)} campus structures; 2 road-connected combat paths.')
