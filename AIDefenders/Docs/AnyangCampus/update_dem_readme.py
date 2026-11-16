from pathlib import Path
p=Path('Docs/AnyangCampus/README.md')
s=p.read_text(encoding='utf-8-sig')
s=s.replace('**원본에는 지형 고도가 없습니다.** 고저차 약 49m, 운동장 고도 43m, 건물 기단과 도로 경사는 사진 기준 게임용 보정값이며 측량 결과가 아닙니다.', '**현재 지형은 제공된 Copernicus GLO-30 DSM을 사용합니다.** 원본 GeoTIFF와 UTM RAW를 4지점에서 대조한 최대 높이 차이는 0.105m로 남북 방향도 확인했습니다. 기존 동/북 좌표에 맞춰 1025×1025 높이맵을 재투영했고 범위는 2400×2050m입니다. 캠퍼스 진입점의 원본 고도 74.347m를 게임 좌표 Y=0으로 삼았습니다. 운동장 높이는 주변 9개 샘플 중앙값인 상대고도 32.94m입니다. DSM은 건물·수목을 포함하는 약 30m 해상도 자료이므로 건물 기단은 주변 낮은 표고로, 운동장과 도로는 게임 통행에 맞게 평탄화했습니다. 측량된 지면이나 세부 계단 고도를 의미하지 않습니다.')
s=s.replace('- 변환: Docs/AnyangCampus/prepare_geo.py (원본 경로는 스크립트 상단).','- 변환: Docs/AnyangCampus/prepare_geo.py 이후 prepare_dem.py 실행. dem_source.py는 RAW 읽기·좌표 변환·GeoTIFF 대조 담당. 입력 패키지 경로는 스크립트 상단.\n- 재현 시 prepare_dem.py는 CampusGeo_PhotoElevation.json 기준으로 다시 계산해 높이가 중복 이동하지 않습니다.\n- DEM_Source.bytes는 보정 전 상대고도, DEM_Graded.bytes는 통행·기단 보정 후 상대고도(float32 little-endian, 남쪽부터 행 저장). 원본 RAW와 설정·라이선스도 Geo 폴더에 보존했습니다.\n- DEM 적용 전 백업: Docs/AnyangCampus/BeforeDEM.')
s=s.replace('두 경로 251개 지점','DSM 적용 후 두 경로 250개 지점').replace('최대 0.36m','최대 0.20m')
s=s.replace('Play에서 대상 69개 생성','DEM 적용 전 Play에서 대상 69개 생성')
s += '\n## 고도 자료 출처\n\n(c) DLR e.V. 2010-2014 and (c) Airbus Defence and Space GmbH 2014-2018\nprovided under COPERNICUS by the European Union and ESA; all rights reserved.\n\n전체 고지문: Assets/Art/AnyangCampus/Geo/README-LICENSE.txt.\nDSM 적용 후 캡처: Docs/CanyonCityUpgrade/Photos/Anyang_DEM_Overview.png.\n'
p.write_text(s,encoding='utf-8')
