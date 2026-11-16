from pathlib import Path
p=Path('Assets/Scripts/Editor/AnyangCampusGeoUpgrade.cs')
s=p.read_text(encoding='utf-8-sig')
for value in ['42.85f','43.04f','43.12f','43.16f','43.1f','43.2f','45.5f']:
    # Only Y coordinates, not pitch Z dimensions/coordinates.
    import re
    s=re.sub(r'(new Vector3\([^,\n]+,)'+re.escape(value)+r'(,)',r'\1FieldY('+value+r')\2',s)
s=s.replace('new Vector3(x,43,','new Vector3(x,FieldHeight,')
s=s.replace('float h=Mathf.Max(1,43-y);','float h=Mathf.Max(1,FieldHeight-y);').replace('new Vector3(x,43-h/2,','new Vector3(x,FieldHeight-h/2,')
s=s.replace('var p=new Vector3(-110,49,-52);','var p=new Vector3(-110,geo.dem!=null?geo.clockHeight:49,-52);')
s=s.replace('new Vector3(-8,24,18),new Vector3(-8,43,-12)','new Vector3(-8,Height(geo,-8,18),18),new Vector3(-8,FieldHeight,-12)')
s=s.replace('terrain.SampleHeight(new Vector3(x,0,z))-bound.min.y*scale','terrain.SampleHeight(new Vector3(x,0,z))+terrain.transform.position.y-bound.min.y*scale')
p.write_text(s,encoding='utf-8')
