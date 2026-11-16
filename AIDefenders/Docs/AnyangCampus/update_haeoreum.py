import json
from pathlib import Path
for name in ['CampusGeo.json','CampusGeo_PhotoElevation.json']:
 p=Path('Assets/Art/AnyangCampus/Geo')/name;g=json.loads(p.read_text(encoding='utf-8'))
 b=next(b for b in g['buildings'] if b['id']=='way/1323539450');b['name']='해오름재';b['height']=35
 p.write_text(json.dumps(g,ensure_ascii=False,separators=(',',':')),encoding='utf-8')
