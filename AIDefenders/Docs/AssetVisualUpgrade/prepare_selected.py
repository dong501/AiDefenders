from pathlib import Path
import json
root = Path(__file__).resolve().parent
selection = {
 '초원_양목장_소품_3종': ['GrasslandFeedTrough','GrasslandHayBales'],
 '초원_과수원_소품_3종': ['GrasslandOrchardWagon','GrasslandAppleCrates','GrasslandScarecrow'],
 '초원_농장경작_9종': ['FarmBeehive','FarmWheelbarrow','FarmWateringCan','FarmToolShed','FarmPumpkinPatch'],
 '초원_특색나무_3종': ['GrasslandAppleTree','GrasslandBirchTree'],
 '초원_숲바닥자연물_9종': ['ForestFallenLog','ForestMushroomCluster','ForestFern'],
 '초원_헛간_풍차_건초_세트': ['Windmill'],
 '사막_유적_소품_세트': ['RuinGate','BrokenColumn','FallenColumn','BrokenRuinWall','RuinObelisk'],
 '사막_마을생활_9종': ['DesertMarketStall','DesertClayJars','DesertWaterCistern','DesertCarpetRack','DesertDatePalm'],
 '군용_천막_보급품_세트': ['FieldTent','SupplyCrate','DrumPallet'],
 '도시_공원시설_9종': ['ParkSlattedBench','ParkCoveredBin','ParkShrubPlanter','ParkPicnicTable','ParkInformationBooth'],
 '도시_폐허건물_9종': ['RuinsStandingFacade','RuinsAbandonedCafe','RuinsCollapsedWarehouse'],
}
models=[]
for pack, names in selection.items():
    source = next((root/'Source'/pack).glob('*MeshData.json'))
    data=json.loads(source.read_text(encoding='utf-8-sig'))
    items = data.get('assets') or data.get('items')
    if not items:
        items=[{'name':n,'parts':[p for p in data['parts'] if p.get('group')==n]} for n in names]
    for item in items:
        if item['name'] not in names: continue
        pivots={p['name']:p.get('position',{'x':0,'y':0,'z':0}) for p in item.get('pivots',[])}
        for part in item['parts']:
            offset=pivots.get(part.get('parent'))
            if offset:
                part['vertices']=[{axis:v[axis]+offset[axis] for axis in ('x','y','z')} for v in part['vertices']]
        verts=[v for p in item['parts'] for v in p['vertices']]
        dims=[round(max(v[a] for v in verts)-min(v[a] for v in verts),2) for a in ('x','y','z')]
        models.append({'name':item['name'],'source':pack,'parts':item['parts']})
        print(item['name'],dims)
target=Path('Assets/Art/EnvironmentExpansion/Source')
target.mkdir(parents=True,exist_ok=True)
(target/'SelectedEnvironmentMeshData.json').write_text(json.dumps({'assets':models},ensure_ascii=False),encoding='utf-8')
(root/'selected_manifest.json').write_text(json.dumps([{'name':m['name'],'source':m['source']} for m in models],ensure_ascii=False,indent=2),encoding='utf-8')
