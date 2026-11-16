from pathlib import Path
import json, zipfile

root = Path(__file__).resolve().parent
selection = {
    'CivilianCarsMeshData.json': ['CitySedan', 'CitySUV'],
    'CompactCarsMeshData.json': ['CityMini', 'CityHatchback'],
    'CityTrafficMeshData.json': ['CityTaxi', 'CityMinivan'],
    'WagonCoupeMeshData.json': ['CityWagon', 'CityCoupe'],
    'UtilityVehiclesMeshData.json': ['CityPickup', 'CityBoxTruck'],
    'DeliveryVanMeshData.json': ['DeliveryVan'],
    'PoliceCarMeshData.json': ['PoliceCar'],
    'AmbulanceMeshData.json': ['Ambulance'],
}
scenery = {
    '도시_생활시설_소품_세트': ['VendingMachine', 'ATMTerminal', 'FireHydrant', 'PostBox', 'PhoneBooth', 'BicycleRack'],
    '도시_공사현장_9종': ['ConstructionScaffold', 'ConstructionCementMixer', 'ConstructionConcretePipe', 'ConstructionCableReel', 'ConstructionRoadworkFence', 'ConstructionWorkLight'],
    '도시_주거골목_9종': ['ResidentialRecyclingBins', 'ResidentialMailboxBank', 'ResidentialTreePlanter'],
    '도시_기본소품_세트': ['StreetTree'],
    '초원_나무_덤불_바위_풀_세트': ['TreeOak', 'BushWide'],
    '검문소_차단기_세트': ['GuardBooth', 'ConcreteBarrier', 'CheckpointSign'],
    '군사기지_통신보급_9종': ['BaseFieldGenerator', 'BaseSupplyPallet'],
    '모래주머니_철조망_세트': ['SandbagStraight', 'BarbedFence', 'WireCoil'],
    '감시탑_탐조등_세트': ['Watchtower', 'GroundSearchlight'],
    '군용_천막_보급품_세트': ['CrateStack', 'JerryCan'],
}
target = Path('Assets/Art/EnvironmentExpansion/Source/SelectedEnvironmentMeshData.json')
models = json.loads(target.read_text(encoding='utf-8-sig'))['assets']
names = {m['name'] for m in models}

def add(data, wanted, source):
    items = data.get('assets') or [{'name': n, 'parts': [p for p in data['parts'] if p.get('group') == n]} for n in wanted]
    for item in items:
        if item['name'] not in wanted or item['name'] in names:
            continue
        pivots = {p['name']: p for p in item.get('pivots', [])}
        def offset(name):
            if name not in pivots:
                return dict(x=0, y=0, z=0)
            p = pivots[name]
            parent = offset(p.get('parent', ''))
            return {a: parent[a] + p['position'][a] for a in ('x','y','z')}
        for part in item['parts']:
            shift = offset(part.get('parent', part.get('group', '')))
            part['vertices'] = [{a:v[a]+shift[a] for a in ('x','y','z')} for v in part['vertices']]
        models.append(dict(name=item['name'], source=source, parts=item['parts']))
        names.add(item['name'])
        print(item['name'])

with zipfile.ZipFile('C:/Users/user/Downloads/포탑제외_전체에셋.zip') as z:
    for filename, wanted in selection.items():
        entry = next(n for n in z.namelist() if n.endswith('/'+filename))
        data = json.loads(z.read(entry).decode('utf-8-sig'))
        if filename == 'AmbulanceMeshData.json':
            # Static wheel mounting positions read from the supplied prefab specification.
            pivots = [{'name':'Wheel'+end+side, 'position':dict(x=-.96 if side=='L' else .96,y=.39,z=1.52 if end=='F' else -1.48)} for end in ('F','R') for side in ('L','R')]
            data = dict(assets=[dict(name='Ambulance', parts=data['parts'], pivots=pivots)])
        add(data, wanted, entry.split('/')[2])
for pack, wanted in scenery.items():
    path = next((root/'Source'/pack).glob('*MeshData.json'))
    add(json.loads(path.read_text(encoding='utf-8-sig')), wanted, pack)
target.write_text(json.dumps(dict(assets=models), ensure_ascii=False), encoding='utf-8')
(root/'selected_manifest.json').write_text(json.dumps([dict(name=m['name'],source=m['source']) for m in models],ensure_ascii=False,indent=2),encoding='utf-8')
