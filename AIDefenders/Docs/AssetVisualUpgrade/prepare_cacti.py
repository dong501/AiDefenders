from pathlib import Path
import copy, json, math

root=Path(__file__).resolve().parent
source=json.loads(next((root/'Source/사막_탐사유적_9종').glob('*MeshData.json')).read_text(encoding='utf-8-sig'))
base=next(a for a in source['assets'] if a['name']=='DesertBranchedCactus')
target=Path('Assets/Art/EnvironmentExpansion/Source/SelectedEnvironmentMeshData.json')
models=json.loads(target.read_text(encoding='utf-8-sig'))['assets']
generated={'DesertRibbedCactus','DesertTallCactus','DesertYoungCactus','DesertDryShrubs'}
models=[a for a in models if a['name'] not in generated]

def part(name,color,vertices,triangles):
    return dict(name=name,color=color,material=name,vertices=[dict(zip('xyz',v)) for v in vertices],triangles=triangles)

for variant in ('Ribbed','Tall','Young'):
    item=copy.deepcopy(base)
    if variant=='Young': item['parts']=[p for p in item['parts'] if p['name'] in ('CactusTrunk','CactusTop')]
    stems=[p for p in item['parts'] if p['name'] in ('CactusTrunk','CactusBranch0','CactusBranch1')]
    for index,stem in enumerate(stems):
        verts=stem['vertices']
        bounds={a:(min(v[a] for v in verts),max(v[a] for v in verts)) for a in 'xyz'}
        cx=sum(bounds['x'])/2;cz=sum(bounds['z'])/2
        rx=(bounds['x'][1]-bounds['x'][0])*.49;rz=(bounds['z'][1]-bounds['z'][0])*.49
        lo,hi=bounds['y']
        for rib in range(8):
            angle=rib*math.pi/4
            x=cx+math.cos(angle)*rx;z=cz+math.sin(angle)*rz
            vv=[]
            for y in (lo+.025,hi-.025):
                for j in range(4):
                    a=j*math.pi/2;vv.append((x+math.cos(a)*.012,y,z+math.sin(a)*.012))
            tt=[]
            for j in range(4):
                k=(j+1)%4;tt.extend([j,k,k+4,j,k+4,j+4])
            item['parts'].append(part('Rib_%d_%d'%(index,rib),'#9bae87',vv,tt))
            for row in range(1,6 if index==0 else 4):
                y=lo+(hi-lo)*row/(6 if index==0 else 4)
                side=(-math.sin(angle)*.018,0,math.cos(angle)*.018)
                vv=[(x+side[0],y-.025,z+side[2]),(x-side[0],y-.025,z-side[2]),(x,y+.026,z),
                    (x+math.cos(angle)*.075,y+.035,z+math.sin(angle)*.075)]
                item['parts'].append(part('Spine_%d_%d_%d'%(index,rib,row),'#d0c4a0',vv,[0,1,3,1,2,3,2,0,3]))
    if variant=='Tall':
        for p in item['parts']:
            for v in p['vertices']:v['x']*=.82;v['z']*=.82;v['y']*=1.22
            if p['color']=='#648971':p['color']='#527b65'
    if variant=='Young':
        for p in item['parts']:
            for v in p['vertices']:v['y']*=.65
        # Small ochre flower on a few young specimens, in the game's low-poly style.
        for petal in range(5):
            a=petal*math.pi*2/5;x=math.cos(a)*.1;z=math.sin(a)*.1;y=1.34
            item['parts'].append(part('FlowerPetal'+str(petal),'#d4a25b',[(x-.055,y,z),(x,y+.06,z),(x+.055,y,z),(x,y,z+.07),(x,y-.025,z)], [0,1,3,1,2,3,2,4,3,4,0,3]))
    item['name']='Desert'+variant+'Cactus';item['source']='사막_탐사유적_9종 + 세로 능선·가시 보강'
    models.append(item)
shrubs=copy.deepcopy(next(a for a in source['assets'] if a['name']=='DesertDryShrubs'))
shrubs['source']='사막_탐사유적_9종';models.append(shrubs)
target.write_text(json.dumps(dict(assets=models),ensure_ascii=False),encoding='utf-8')
(root/'selected_manifest.json').write_text(json.dumps([dict(name=m['name'],source=m['source']) for m in models],ensure_ascii=False,indent=2),encoding='utf-8')
print('Prepared 3 ribbed cactus variants and dry shrubs.')
