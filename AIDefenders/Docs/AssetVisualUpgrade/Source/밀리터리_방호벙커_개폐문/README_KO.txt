AI Defenders - 밀리터리 방호 벙커

단순화된 석재/콘크리트 방호 건물, 내부 공간, 두꺼운 지붕, 측면 보강대,
환기구, 출입 경사로, 좌우 슬라이딩 방폭문 포함. 788 삼각형.
폭8.6m, 높이3.58m, 경사로 포함 깊이7.85m. 정면 +Z, 바닥 Y=0.
바닥 상단 Y=0.16, 천장 하단 Y=2.72. 입구 폭3.2m, 높이2.4m.

Unity 적용:
1. Assets/AIDefendersBunker 폴더를 프로젝트 Assets로 복사.
2. Tools > AI Defenders > Create Bunker 실행.
3. Assets/AIDefendersBunkerGenerated 안의 Bunker.prefab 또는 BunkerDoorDemo.prefab 배치.
기존 출력 폴더가 있으면 새 경로를 생성한다.
Built-in Standard / URP Lit 재질 생성, HDRP는 별도 재질 설정 필요.
범용 OBJ + MTL, 모델 생성 소스, 미리보기 PNG/GIF 포함. FBX 미포함.
OBJ는 문이 닫힌 정적 모델이다. 움직임은 Unity 생성 프리팹의 스크립트로 구현.

문 제어:
BunkerDoors.Open(), Close(), Toggle(), SetOpen(bool).
startOpen: 실행 시작 시 열린 상태. autoDemo: 3초 간격으로 열림/닫힘 전환.
기본 프리팹은 닫힌 상태에서 대기. DoorDemo 프리팹은 자동 반복.
문짝은 각각1.65m 이동, 기본1.1m/s. 열림/닫힘 소요 약1.5초.
문짝마다 키네마틱 Rigidbody, 패널 BoxCollider 사용. FixedUpdate에서 MovePosition.
문은 벽 속으로 수납되는 시각 표현이며, 벽 내부 수납 포켓은 별도 불리언 모델링하지 않았다.
벽/바닥/지붕/경사로는 부품별 정적 MeshCollider 사용. 내부를 루트 박스로 막지 않는다.
문 끼임 감지, 자동 재개방, 출입 인식, 피해 처리, NavMesh 갱신, 소리는 미포함.
경사로와 출입문을 사용하는 이동 경로/AI는 프로젝트에서 연결해야 한다.
정면 표시등은 색상 장식이며 실제 조명/점멸 기능은 없다.

Preview.png: 닫힌 문. OpenPreview.png: 열린 문.
Cutaway.png: 내부 구조 확인을 위해 지붕과 한쪽 벽 일부를 미리보기에서만 숨긴 그림.
실제 프리팹/OBJ에는 지붕 및 양쪽 벽 모두 포함.
DoorPreview.gif: 실제 메시 기반 개폐 모션 미리보기.
모든 미리보기는 소프트웨어 렌더이며 Unity 화면 캡처가 아니다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
