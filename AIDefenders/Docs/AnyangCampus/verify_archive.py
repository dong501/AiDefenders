from pathlib import Path
import re,json
base=Path('MapBackups/AnyangCampus_20261116');guids={}
for p in (base/'Assets').rglob('*.meta'):
 m=re.search(rb'^guid: ([0-9a-f]{32})',p.read_bytes(),re.M)
 if m:guids[m[1]]=str(p.relative_to(base))
refs=[]
for p in Path('Assets').rglob('*'):
 if p.suffix.lower() not in {'.unity','.prefab','.mat','.asset','.terrainlayer'}:continue
 for g in set(re.findall(rb'guid: ([0-9a-f]{32})',p.read_bytes()))&guids.keys():refs.append((str(p),guids[g]))
print(json.dumps(refs,ensure_ascii=True))
assert not refs,'Other active assets still reference archived files'
