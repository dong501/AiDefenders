from pathlib import Path
import zipfile
root=Path('MapBackups/AnyangCampus_20261116');dest=root.with_suffix('.zip')
assert not dest.exists(), 'Archive already exists'
with zipfile.ZipFile(dest,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in root.rglob('*'):
  if p.is_file():z.write(p,p.relative_to(root.parent))
with zipfile.ZipFile(dest) as z:
 assert z.testzip() is None
 print('ZIP verified:',len(z.infolist()),'files;',round(dest.stat().st_size/1024/1024,1),'MB')
