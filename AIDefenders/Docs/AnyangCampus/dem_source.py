"""Read supplied bottom-up UTM RAW and cross-check with the source GeoTIFF."""
import json, math
from pathlib import Path
import numpy as np
from PIL import Image

PACKAGE=Path(r'C:\Users\user\Downloads\Anyang_Unity_Terrain_Package')
SETTINGS=json.loads((PACKAGE/'AnyangTerrain_Settings.json').read_text())
RAW=np.fromfile(PACKAGE/'AnyangTerrain_1025.raw',dtype='<u2').reshape(1025,1025)

def utm(lon,lat):
    # WGS84 transverse Mercator, zone 52N (central meridian 129 degrees).
    a=6378137.;e2=.0066943799901413165;k=.9996
    p=np.radians(lat);l=np.radians(lon-129);ep=e2/(1-e2)
    n=a/np.sqrt(1-e2*np.sin(p)**2);t=np.tan(p)**2;c=ep*np.cos(p)**2;A=np.cos(p)*l
    m=a*((1-e2/4-3*e2**2/64-5*e2**3/256)*p-(3*e2/8+3*e2**2/32+45*e2**3/1024)*np.sin(2*p)+(15*e2**2/256+45*e2**3/1024)*np.sin(4*p)-(35*e2**3/3072)*np.sin(6*p))
    x=500000+k*n*(A+(1-t+c)*A**3/6+(5-18*t+t*t+72*c-58*ep)*A**5/120)
    y=k*(m+n*np.tan(p)*(A*A/2+(5-t+9*c+4*c*c)*A**4/24+(61-58*t+t*t+600*c-330*ep)*A**6/720))
    return x,y

def bilinear(data,x,y):
    x=np.clip(x,0,data.shape[1]-1.000001);y=np.clip(y,0,data.shape[0]-1.000001)
    i=np.floor(x).astype(int);j=np.floor(y).astype(int);fx=x-i;fy=y-j
    return data[j,i]*(1-fx)*(1-fy)+data[j,i+1]*fx*(1-fy)+data[j+1,i]*(1-fx)*fy+data[j+1,i+1]*fx*fy

def sample(lon,lat):
    x,y=utm(lon,lat);b=SETTINGS['projected_bounds_m']
    ix=(x-b['xmin_easting'])/(b['xmax_easting']-b['xmin_easting'])*1024
    iz=(y-b['ymin_northing'])/(b['ymax_northing']-b['ymin_northing'])*1024
    return 15+bilinear(RAW,ix,iz)*200/65535

def local(x,z):
    return sample(126.9200+np.asarray(x)/(111320*math.cos(math.radians(37.3913))),37.3913+np.asarray(z)/111320)

if __name__=='__main__':
    tif=Image.open(r'C:\Users\user\Downloads\Copernicus_DSM_COG_10_N37_00_E126_00_DEM\Copernicus_DSM_COG_10_N37_00_E126_00_DEM.tif')
    d=np.array(tif);lon=np.array([126.918,126.920,126.922,126.914]);lat=np.array([37.390,37.392,37.394,37.387])
    direct=bilinear(d,(lon-126)*3600,(38-lat)*3600)
    from_raw=sample(lon,lat)
    print('RAW vs GeoTIFF absolute heights:',from_raw,direct,'max difference',np.max(abs(from_raw-direct)))
    for x,z in [(53,99),(3,86),(-3,53),(34,19),(-45,12),(-56,-24),(-112,-56),(-119,-102),(95,35),(107,-3)]:
        print(x,z,round(float(local(x,z)),2))
