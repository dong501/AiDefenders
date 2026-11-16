# 안양대학교 히든 스테이지

진입: 메인 메뉴 → 게임 시작 → 챕터 카드 아래 **히든 · 안양대학교** → 스테이지 선택 → 전투 시작.

## 지리 자료 반영 (2026-11-16)

제공된 export.geojson의 건물 81개(분리된 다중 폴리곤 포함 82개 윤곽)와 도로 89개를 반영했습니다. 동쪽 X, 북쪽 Z, 1 Unity 단위당 약 1m입니다. 원본은 Assets/Art/AnyangCampus/Geo/Source.geojson, 변환 자료는 같은 폴더 CampusGeo.json입니다.

수리관·대신관의 긴 윤곽, 수봉관의 꺾인 외곽, 문화관, 상부 아리관·아리비전센터·일우중앙도서관·창의융합산학관의 상대 위치와 경사 진입로를 재배치했습니다. 주변 아파트와 일반도로도 자료 범위 내에서 생성했습니다. 오목한 지붕은 귀 자르기 방식으로 삼각 분할하여 외곽을 보존합니다.

**현재 지형은 제공된 Copernicus GLO-30 DSM을 사용합니다.** 원본 GeoTIFF와 UTM RAW를 4지점에서 대조한 최대 높이 차이는 0.105m로 남북 방향도 확인했습니다. 기존 동/북 좌표에 맞춰 1025×1025 높이맵을 재투영했고 범위는 2400×2050m입니다. 캠퍼스 진입점의 원본 고도 74.347m를 게임 좌표 Y=0으로 삼았습니다. 운동장 높이는 주변 9개 샘플 중앙값인 상대고도 32.94m입니다. DSM은 건물·수목을 포함하는 약 30m 해상도 자료이므로 건물 기단은 주변 낮은 표고로, 운동장과 도로는 게임 통행에 맞게 평탄화했습니다. 측량된 지면이나 세부 계단 고도를 의미하지 않습니다. 높이·층수 태그가 있는 주변 건물은 해당 값을 사용하고 없는 건물은 기본 높이를 사용합니다. 일부 건물 이름과 외관은 사진 기반 단순화입니다. 모든 보행로가 수록된 자료는 아니므로 완전한 실측 복제는 아닙니다.

출처: © OpenStreetMap contributors, ODbL 1.0. 원본 파일의 출처 정보를 유지하며 게임 배포 시에도 출처 표시를 포함해야 합니다.

## 구현 및 재생성

- 장면: Assets/Scenes/GameStages/AnyangCampus/AnyangCampus_1.unity
- 웨이브: Assets/Art/AnyangCampus/AnyangCampus_Day.asset
- 2개 진입 경로, 운동장 앞 방어 지점, 3웨이브, 시작 자금 650, 기지 체력 30.
- 기존 모델 선택·포탑 설치·업그레이드 흐름 사용.
- 변환: Docs/AnyangCampus/prepare_geo.py 이후 prepare_dem.py 실행. dem_source.py는 RAW 읽기·좌표 변환·GeoTIFF 대조 담당. 입력 패키지 경로는 스크립트 상단.
- 재현 시 prepare_dem.py는 CampusGeo_PhotoElevation.json 기준으로 다시 계산해 높이가 중복 이동하지 않습니다.
- DEM_Source.bytes는 보정 전 상대고도, DEM_Graded.bytes는 통행·기단 보정 후 상대고도(float32 little-endian, 남쪽부터 행 저장). 원본 RAW와 설정·라이선스도 Geo 폴더에 보존했습니다.
- DEM 적용 전 백업: Docs/AnyangCampus/BeforeDEM.
- Unity 재생성: AnyangCampusGeoUpgrade.Apply() 또는 Tools → AI Defenders → Apply Georeferenced Anyang Campus. Play를 종료하고 실행합니다.
- 변경 전 백업: Docs/AnyangCampus/BeforeGeo/AnyangCampus_1.unity.
- 이전 사진 기반 생성기는 변경 전 버전이며 현재 배치를 유지하려면 Geo 생성기를 사용합니다.

## 검증

Unity 컴파일 및 Console 오류 없음. 스크립트 누락 없음. 스폰 그룹·프리팹·카메라 참조 및 빌드 목록 확인. DSM 적용 후 두 경로 250개 지점에서 건물 지붕 관통 없음, 지면 높이 차이 최대 0.20m. 80개 콜라이더 표면에서 플랫폼 면적 지지 검사 통과.

DEM 적용 전 Play에서 대상 69개 생성, 추적 대상 36개 이동 및 경사 상승 확인(최대 고도 증가 5.86m). 모델 선택 대기를 런타임에서만 우회하여 이동을 검사했습니다. YOLO 서버와 인식 정확도는 이번 작업에서 검증하지 않았습니다. 종료 후 메인 메뉴 편집 상태로 복원했습니다.

검증: Docs/AnyangCampus/verify.cs.txt.
미리보기: Assets/Resources/UI/Previews/AnyangCampus_1.png.
실제 캡처: Docs/CanyonCityUpgrade/Photos/Anyang_Geo_Overview.png 및 Anyang_Geo_Hillside.png.

## 고도 자료 출처

(c) DLR e.V. 2010-2014 and (c) Airbus Defence and Space GmbH 2014-2018
provided under COPERNICUS by the European Union and ESA; all rights reserved.

전체 고지문: Assets/Art/AnyangCampus/Geo/README-LICENSE.txt.
DSM 적용 후 캡처: Docs/CanyonCityUpgrade/Photos/Anyang_DEM_Overview.png.

DSM 적용 후 추가 검증: Unity Terrain 25개 격자 지점과 보정 높이 파일의 차이 최대 0.0037m로 방향·스케일 확인. Play에서 대상 49개 생성, 추적 대상 29개 이동, 9개 상승(최대 고도 증가 18.45m) 확인. 메인 메뉴 편집 상태 복원.

## 지형 접합 및 사진 기반 외관 수정

도로·인도의 양쪽 모서리를 최종 Terrain에 투영하고, 노반과 건물 기단을 지형 아래까지 연장했습니다. 도로 교차부의 지형 보정은 가장 가까운 도로 기준으로 통합하며, 인도 아래 평탄 구간과 완충 경사를 넓혔습니다. 저장된 장면의 도로·인도 꼭짓점 280,260개에서 지정 이격(아스팔트 8cm, 인도 22cm)을 검증했고 기단 하단 21,747개가 지형 아래에 닿습니다. 두 적 경로 250개 샘플의 지면 차이 최대 0.33m, 플랫폼 지지 표면 118개 검사 통과.

수리관·대신관에 갈색 세로 패널, 수봉관 곡면부에 유리 커튼월과 층별 프레임, 대신관에 삼각형 입구, 해오름재에 높이와 밝은 계단실 외벽·발코니를 사진 기준으로 추가했습니다. 건물명은 외벽에 부착했습니다. 주변 도로에는 회색 아스팔트·차선·인도·횡단보도·신호등 구조물을 적용했습니다. 사진의 특징을 단순화한 게임용 모델이며 실사 수준 복제를 의미하지 않습니다.

현재 캡처: Docs/CanyonCityUpgrade/Photos/Anyang_Architecture_Fixed.png, Anyang_Street_Fixed.png, Anyang_Grounded_Overview.png.
접합 검증: Docs/AnyangCampus/verify_grounded.cs.txt.
