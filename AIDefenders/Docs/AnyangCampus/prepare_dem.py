"""Align supplied elevation with the existing east/north metre grid and grade gameplay surfaces."""
import json, shutil, math
from pathlib import Path
import numpy as np
from dem_source import local, SETTINGS, PACKAGE

OUT=Path('Assets/Art/AnyangCampus/Geo')
path=OUT/'CampusGeo.json';g=json.loads(path.read_text(encoding='utf-8'))
baseline=OUT/'CampusGeo_PhotoElevation.json'
if not baseline.exists():shutil.copyfile(path,baseline)
# Always start from the immutable photo-era geometry, avoiding cumulative offsets.
g=json.loads(baseline.read_text(encoding='utf-8'))
for name in ['AnyangTerrain_Settings.json','README-LICENSE.txt']:
    shutil.copyfile(PACKAGE/name,OUT/name)
shutil.copyfile(PACKAGE/'AnyangTerrain_1025.raw',OUT/'AnyangTerrain_1025.raw.bytes')
reference=float(local(53,99))
def height(x,z):return local(x,z)-reference

field=float(np.median([height(x,z) for x in [-85,-56,-25] for z in [-40,-24,-8]]))
for b in g['buildings']:
    # DSM on a roof is not its foundation: use the low surrounding skirt for bases.
    pts=b['points'];xs=[p['x'] for p in pts];zs=[p['z'] for p in pts]
    samples=[float(height(x,z)) for x in [min(xs)-7,max(xs)+7] for z in np.linspace(min(zs)-7,max(zs)+7,5)]
    samples += [float(height(x,z)) for z in [min(zs)-7,max(zs)+7] for x in np.linspace(min(xs)-7,max(xs)+7,5)]
    b['baseY']=round(float(np.percentile(samples,30)),3)
    if b['kind']=='bridge': b['baseY']+=12
    for p in pts:p['y']=round(float(height(p['x'],p['z'])),4)
for r in g['roads']:
    for p in r['points']:p['y']=round(float(height(p['x'],p['z'])),4)
    # Both road representations share the exact XY at junctions and sample the same DSM.
main=next(r for r in g['roads'] if r['id']=='way/1323539454')
entry=min(main['points'],key=lambda p:abs(p['x']+45)+abs(p['z']-12))
entry['y']=round(field+.10,4)
for route in g['paths']:
    for p in route['points']:p['y']=round(float(height(p['x'],p['z'])),4)
    route['points'][-2]=dict(entry)
    route['points'][-1]['y']=round(field+.20,4)
g['fieldHeight']=round(field,4)
g['clockHeight']=round(float(height(-110,-52)),4)
g['elevationSource']='Copernicus GLO-30 DSM; building skirts, sports terrace and roads graded for gameplay; not a surveyed bare-earth model.'

b=SETTINGS['geojson_bounds_wgs84'];scale=111320*math.cos(math.radians(37.3913))
x0=math.floor(((b[0]-126.92)*scale-100)/10)*10;z0=math.floor(((b[1]-37.3913)*111320-100)/10)*10
w=math.ceil(((b[2]-126.92)*scale+100-x0)/10)*10;length=math.ceil(((b[3]-37.3913)*111320+100-z0)/10)*10
n=1025;X,Z=np.meshgrid(np.linspace(x0,x0+w,n),np.linspace(z0,z0+length,n))
source=height(X,Z).astype('<f4');Y=source.copy()
def smooth(v):v=np.clip(v,0,1);return v*v*(3-2*v)
def segment(a,b):
    dx=b['x']-a['x'];dz=b['z']-a['z'];l=dx*dx+dz*dz
    f=np.clip(((X-a['x'])*dx+(Z-a['z'])*dz)/max(l,1e-9),0,1)
    return np.hypot(X-a['x']-dx*f,Z-a['z']-dz*f),a['y']+(b['y']-a['y'])*f
for building in g['buildings']:
    if building['kind'] in ['bridge','roof']:continue
    pts=building['points'];inside=np.zeros(X.shape,dtype=bool);dist=np.full(X.shape,np.inf)
    for a,b in zip(pts,pts[1:]+pts[:1]):
        inside ^= ((a['z']>Z)!=(b['z']>Z)) & (X<(b['x']-a['x'])*(Z-a['z'])/((b['z']-a['z']) or 1e-12)+a['x'])
        d,_=segment(a,b);dist=np.minimum(dist,d)
    t=smooth(dist/(12 if building['campus'] else 6));t[inside]=0
    Y=building['baseY']*(1-t)+Y*t
distance=np.hypot(np.maximum.reduce([-104-X,np.zeros(X.shape),X+8]),np.maximum.reduce([-53-Z,np.zeros(X.shape),Z-6]))
t=smooth(distance/14);Y=field*(1-t)+Y*t
road_distance=np.full(X.shape,np.inf);road_elevation=np.zeros(X.shape);road_blend=np.ones(X.shape)
for r in g['roads']:
    closest=np.full(X.shape,np.inf);elev=np.zeros(X.shape)
    for a,b in zip(r['points'],r['points'][1:]):
        d,h=segment(a,b);mask=d<closest;closest[mask]=d[mask];elev[mask]=h[mask]
    # Keep the entire sidewalk on the graded shoulder, including Terrain interpolation cells.
    t=smooth((closest-r['width']-3.5)/7)
    # Grade intersecting roads together; a later road must not cut the earlier one's bed.
    score=closest/max(r['width'],1);mask=score<road_distance
    road_distance[mask]=score[mask];road_elevation[mask]=elev[mask];road_blend[mask]=t[mask]
Y=(road_elevation-.10)*(1-road_blend)+Y*road_blend
dem=dict(resolution=n,x=x0,z=z0,width=w,length=length,y=-64,height=256,referenceAbsoluteElevation=reference,
         source='DEM_Source.bytes',graded='DEM_Graded.bytes',attribution='(c) DLR e.V. 2010-2014 and (c) Airbus Defence and Space GmbH 2014-2018 provided under COPERNICUS by the European Union and ESA; all rights reserved.')
assert np.isfinite(Y).all() and Y.min()>=-64 and Y.max()<192
source.tofile(OUT/dem['source']);Y.astype('<f4').tofile(OUT/dem['graded'])
g['dem']=dem
path.write_text(json.dumps(g,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
print(f'DSM applied: {n}x{n}, {w}x{length}m; reference absolute {reference:.2f}m; field relative {field:.2f}m; terrain range {Y.min():.2f}..{Y.max():.2f}m')
