# 도시 중앙 쉘터 / 4방향 방어

도시 1스테이지를 중앙 교차로의 대피소 방어 구조로 변경했습니다.

- 목표: (0, -100) 중앙 쉘터. 동·서·남·북에 각각 135m 떨어진 스폰을 두고 6개 경유점을 거쳐 같은 목표로 진입합니다.
- 쉘터: 네 방향 출입구, 콘크리트 방호벽, 노란 안전 표시, 의료 십자 바닥 표시. 기존 대피소·의료 시설을 이전했습니다.
- 차량: 승용차·SUV·소형차·해치백·택시·미니밴·왜건·쿠페·픽업·배송 밴·경찰차·화물차 12종, 36대 추가.
- 생활 소품: 소화전·재활용함·우편함·자판기·ATM·전화 부스·자전거 거치대.
- 공사 구역: 비계·믹서·콘크리트 관·케이블 릴·작업등·안전 펜스.
- 카메라 시작 위치와 드론 이동 제한을 중앙 방어 구역에 맞췄습니다.
- 선택 화면 설명을 사방 방어에 맞춰 수정했습니다.

## 검증
- 5개 웨이브 총 등장 수: 52 / 52 / 40 / 73 / 92로 이전과 동일. 등장 시각과 대상 종류를 유지하면서 방향별로 분배했습니다.
- 저장 후 씬을 다시 열어 스포너·경로·목표 참조를 확인했습니다.
- 플레이 모드에서 네 방향에 한 개씩 출현시킨 대상 4개가 모두 목표에 도달하고 기지 피해가 4회 처리되는 것을 확인했습니다. 테스트에서는 복제한 임시 StageData를 사용했습니다.
- 추가 장식물에는 충돌체가 없고 재질 누락 0개입니다.
- YOLO 서버를 연결한 실전 전투 및 5개 웨이브 전체 밸런스 검증은 별도로 필요합니다.

## 파일
- 씬: `Assets/Scenes/GameStages/City/City_1.unity`
- 웨이브: `Assets/Waves/StageData_City_1.asset`
- 에디터 빌더: `Assets/Scripts/Editor/CityFourWayUpgrade.cs`
- 초기 상태 백업: `Before/` (빌더 재실행 시 덮어쓰지 않음)
- [전체 화면](../AssetVisualUpgrade/City_FourWay.png)
- [중앙 쉘터](../AssetVisualUpgrade/City_FourWay_Shelter.png)
- [배치 전경](../AssetVisualUpgrade/City_FourWay_Top.png)

재배치는 Unity 메뉴 `Tools > AI Defenders > Environment Expansion > 3 Upgrade City Four Way Defense`에서 실행합니다. 수동 조정한 추가 소품은 빌더를 실행하면 초기 배치로 돌아갑니다. 원본 ZIP의 정적 메시만 사용하며 외부 스크립트는 실행하지 않습니다. 데이터 준비 스크립트는 기존 `prepare_selected.py` 이후 `prepare_city_expansion.py` 순서입니다.

## 시작 시 검은 화면 수정
드론 본체 회전을 (40, 180, 0), 두 자식 카메라의 로컬 회전을 (0, 0, 0)으로 정렬했습니다. 카메라가 본체 앞에 1.45m 위치하며, 준비 및 전투 단계에서 실제 게임 카메라로 정상 화면을 렌더링한 것을 확인했습니다. Camera_Preparation.png와 Camera_Combat.png 참조.


## 도시 나무 보강
기존 가로수 32개를 StreetTree·참나무·자작나무로 교체하고, 화분형 나무 10개와 관목 12개를 보도에 추가했습니다. 기존 나무는 Street_Trees 루트 아래 비활성 상태로 보관되며 새 배치는 City_Greenery_Upgrade 아래 있습니다. 추가 장식물의 충돌체·재질 누락·레이어 오류는 0개이고, 스폰·목표·경유점 Transform 비교도 일치합니다. 카메라 본체 방향과 자식 카메라 정렬도 유지됐습니다. 재배치는 Tools > AI Defenders > Environment Expansion > 4 Upgrade City Trees에서 실행할 수 있습니다.
[가로수 미리보기](../AssetVisualUpgrade/City_Greenery_Street.png)


## 좀비 영화풍 긴급 쉘터 보강
경찰차 총 2대와 구급차 2대, 의료 천막, 검문 부스, 철조망 방호벽, 모래주머니, 감시탑, 탐조등 4곳, 발전기, 보급품을 배치했습니다. 네 방향 입구에는 철제 프레임·경고등·안내판을 추가했습니다. 중앙 통행 공간을 비웠으며, 스폰·목표·경로·카메라 Transform을 유지했습니다. 추가 장식물에는 충돌체가 없습니다. 구급차는 ZIP의 정적 메시와 바퀴 장착 좌표만 사용했습니다.
[쉘터 입구](../AssetVisualUpgrade/City_Shelter_Entrance.png) · [의료 구역](../AssetVisualUpgrade/City_Shelter_Medical.png)
재배치 메뉴: Tools > AI Defenders > Environment Expansion > 5 Upgrade Emergency Shelter.

