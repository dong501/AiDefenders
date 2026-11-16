# 제공 에셋을 활용한 챕터 배경 보강

## 적용 결과
- 초원 낮·저녁·밤: 각 34개. 목장 먹이통과 건초, 사과 수레, 농기구 창고, 풍차, 호박밭, 과수원·벌통, 자작나무·고사리·버섯·쓰러진 통나무.
- 사막 낮: 18개. 방어 거점 천막과 보급품, 유적 문·기둥·오벨리스크, 시장 가판대·항아리·물 저장고·야자수.
- 도시: 22개. 대피소 안내 부스·벤치·쓰레기통·보급품, 화단·피크닉 테이블, 폐카페·붕괴 창고·파손된 건물 벽.

기존 스폰·경유점·방어 목표·카메라 Transform 비교가 모두 일치합니다. 초원에는 군용 차량이나 군사 시설을 추가하지 않았습니다. 기존 도로, 시간대 조명, 도시 안개를 유지했습니다.

## 에디터에서 확인
해당 스테이지 씬을 열고 Hierarchy의 `Environment_Asset_Expansion`을 선택한 뒤 F 키로 이동합니다. 개별 소품은 이 루트 아래에서 옮기거나 삭제할 수 있습니다.

## 미리보기
- [초원 낮](Grassland_Ranch.png), [저녁](Grassland_Evening.png), [밤](Grassland_Night.png)
- [사막 거점과 유적](Desert_Outpost.png)
- [도시 대피소](City_Shelter.png), [도시 폐허](City_Ruins.png)

## 검증
37종의 정적 프리팹을 생성했습니다. 각 소품은 색상별 서브메시를 합쳐 MeshRenderer 하나를 사용합니다. 추가 소품의 충돌체 0개, 누락 재질 0개, Ignore Raycast 레이어 오류 0개를 확인했습니다. Unity 컴파일 오류가 없고 5개 스테이지를 직접 렌더링했습니다. 전체 웨이브 플레이 테스트는 수행하지 않았습니다.

`scene_validation.json`에는 스테이지별 배치 수와 변경 전후 게임플레이 Transform이 기록되어 있습니다. `Before`는 작업 직전 씬 백업입니다.

## 원본과 재생성
ZIP의 정적 메시 JSON만 읽었으며 ZIP 내부 스크립트는 실행하지 않았습니다. 선택 목록은 `selected_manifest.json`, Unity 입력 데이터는 `Assets/Art/EnvironmentExpansion/Source/SelectedEnvironmentMeshData.json`입니다.

Unity 메뉴 `Tools > AI Defenders > Environment Expansion`:
1. `1 Import Selected Assets`: 메시·재질·프리팹 생성/갱신.
2. `2 Apply To Chapters`: 5개 씬의 추가 소품 루트를 재배치하고 저장. 수동으로 옮긴 추가 소품도 초기 배치로 돌아가므로 필요할 때만 실행하세요.

도시 추가 변경: [중앙 쉘터 4방향 방어 및 차량·거리 소품 보강](../CityFourWay/README.md). 선택 프리팹은 현재 총 63종입니다.
