from pathlib import Path
import zipfile, json

archive = Path('C:/Users/user/Downloads/포탑제외_전체에셋.zip')
root = Path(__file__).resolve().parent
packs = []
categories = ('초원_배경', '사막_배경', '도시_배경', '군사기지_시설')
with zipfile.ZipFile(archive) as z:
    for name in z.namelist():
        bits = name.split('/')
        if len(bits) < 4 or bits[1] not in categories or not name.endswith('MeshData.json'):
            continue
        data = json.loads(z.read(name).decode('utf-8-sig'))
        items = data.get('assets') or data.get('items') or []
        groups = sorted(set(p.get('group', '') for p in data.get('parts', [])))
        info = {'category': bits[1], 'pack': bits[2], 'entry': name,
                'keys': list(data), 'items': [i.get('name') for i in items], 'groups': groups,
                'parts': sum(len(i.get('parts', [])) for i in items) or len(data.get('parts', []))}
        packs.append(info)
        directory = root / 'Source' / bits[2]
        directory.mkdir(parents=True, exist_ok=True)
        (directory / Path(name).name).write_bytes(z.read(name))
        for suffix in ('Preview.png', 'SampleLayout.png', 'README_KO.txt'):
            match = '/'.join(bits[:3]) + '/' + suffix
            if match in z.namelist():
                (directory / suffix).write_bytes(z.read(match))
(root / 'catalog.json').write_text(json.dumps(packs, ensure_ascii=False, indent=2), encoding='utf-8')
for p in packs:
    if p['category'] in ('초원_배경', '사막_배경') or any(s in p['pack'] for s in ('공원', '폐허', '기본소품', '기본시설', '천막')):
        print(p['pack'], ':', ', '.join(p['items'] or p['groups']), 'parts=', p['parts'])
