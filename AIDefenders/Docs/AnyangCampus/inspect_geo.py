import json, math
from pathlib import Path
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.font_manager import FontProperties

source = Path(r'C:\Users\user\Downloads\export.geojson')
data = json.loads(source.read_text(encoding='utf-8-sig'))
origin = (126.9200, 37.3913)
def project(p):
    return ((p[0]-origin[0])*111320*math.cos(math.radians(origin[1])), (p[1]-origin[1])*111320)
fig, ax = plt.subplots(figsize=(14,14))
font=FontProperties(fname='C:/Windows/Fonts/malgun.ttf')
for f in data['features']:
    g,p=f['geometry'],f['properties']
    if g['type']=='LineString':
        pts=[project(x) for x in g['coordinates']]
        ax.plot(*zip(*pts),color='#bbb',linewidth=2)
    elif g['type']=='Polygon':
        pts=[project(x) for x in g['coordinates'][0]]
        if not (-260 < sum(x for x,y in pts)/len(pts)<180 and -180<sum(y for x,y in pts)/len(pts)<190): continue
        ax.fill(*zip(*pts),color='#b4d5df' if p.get('building')=='university' else '#ddd',edgecolor='#345')
        x=sum(x for x,y in pts)/len(pts);y=sum(y for x,y in pts)/len(pts)
        name=p.get('name','')+'\n'+f['id'].split('/')[-1]
        ax.text(x,y,name,ha='center',fontsize=8,fontproperties=font)
        print(f['id'],p, 'center',round(x),round(y))
ax.set_xlim(-260,180);ax.set_ylim(-180,190);ax.set_aspect('equal');ax.grid(alpha=.2)
fig.savefig('Docs/AnyangCampus/Geo_Footprints.png',dpi=150,bbox_inches='tight')
