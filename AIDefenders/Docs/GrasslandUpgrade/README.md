# 초원 챕터 배경 보강

초원 낮/저녁/밤 씬에 `Grassland_Scenery_Upgrade` 프리팹을 적용했습니다.

- 기존 Pine Trees의 소나무·덤불·풀·바위·그루터기·부러진 나무를 활용한 자연 군락.
- 기존 농장 에셋의 농기구·수레바퀴·표지판·울타리, 대장간 에셋의 나무 상자로 목장 작업 공간 구성.
- 양·늑대 스테이지 분위기에 맞춰 차량과 차량용 지원 공간은 배치하지 않습니다.
- 건초 더미, 돌 우물, 야생화 및 저녁/밤용 목장 등불 추가.
- 초원 3종의 스테이지 카드/메인 메뉴용 실제 맵 미리보기 갱신.

기존 Terrain 데이터, 적 경유점 21개, 스폰 오브젝트와 방어 목표를 변경하지 않았습니다. 자연 군락은 경로 중심에서 13m, 풀은 9m, 야생화는 10m 이상 떨어뜨리고 호수 아래 지형과 급경사를 제외했습니다. 추가 장식의 충돌체는 비활성 상태입니다.

검증: 세 씬의 렌더링 및 재질 누락 없음, 차량·지원 공간 0개, 목장 작업 공간과 우물 유지, 낮 등불 0개/저녁·밤 1개 활성. 방어 목표와 경유점을 변경하지 않았으며 추가 장식의 활성 충돌체는 0개입니다. 배경 렌더러는 348개입니다. 전체 웨이브 플레이 테스트는 이번 배경 작업에 포함하지 않았습니다.

재생성: Unity 메뉴 `Tools > AI Defenders > Upgrade Grassland Scenery`.
생성 원본: `Assets/Scripts/Editor/GrasslandSceneryBuilder.cs`.
배치 프리팹: `Assets/Scenes/GameStages/GrassField/SceneryUpgrade/GrasslandScenery.prefab`.

스크린샷: `Grassfield_1_Ranch.png`(목장 주변), `Grassfield_1_Overview.png`(낮 전경), `Grassfield_2_Overview.png`(저녁), `Grassfield_3_Overview.png`(밤).
