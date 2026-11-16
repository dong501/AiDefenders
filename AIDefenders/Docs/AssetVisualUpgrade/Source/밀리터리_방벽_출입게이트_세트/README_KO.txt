AI Defenders - 밀리터리 방벽 출입 게이트 세트

6종: Wall4m(4m 방벽), Wall2m(2m 방벽), WallCorner(L자 코너),
WallDamaged4m(파손 방벽), Gate8m(8m 출입 게이트), WallEndPost(마감 기둥).
1,180 삼각형. 바닥 Y=0, 1단위=1m. 일반 방벽 높이2.4m.
외부 텍스처 없는 단색 재질. OBJ+MTL, 소스, 미리보기 및 연결 정보 포함.
FBX/unitypackage는 포함하지 않는다.

Unity 적용:
1. Assets/AIDefendersPerimeterWalls를 프로젝트 Assets로 복사.
2. Tools > AI Defenders > Create Perimeter Walls 실행.
3. Assets/AIDefendersPerimeterWallsGenerated 아래 개별 프리팹/연결 배치 예제/Gate8mDemo 사용.
기존 출력 폴더가 있으면 새 경로 생성. Built-in Standard / URP Lit 지원.
HDRP는 별도 재질 설정 필요.

연결 기준 (ConnectionInfo.json 참고):
4m 방벽은 X방향 양 끝 +-2m, 2m 방벽은 +-1m, 게이트는 +-4m.
코너는 로컬(2,0,0), (0,0,2)가 연결 끝이며 회전해 방향 변경 가능.
예제는 게이트 양쪽 방벽, 우측 코너와 꺾인 방벽, 좌측 파손 방벽/짧은 방벽/마감 기둥 포함.
장식 돌출과 마감 기둥은 연결부에서 조금 겹치도록 설계했다.
파손 방벽은 이미 파손된 고정 모델이며 실시간 붕괴 기능은 없다.

게이트:
PerimeterGate.Open(), Close(), Toggle(), SetOpen(bool) 제공.
startOpen/autoDemo 설정 가능. 기본 프리팹은 닫힌 상태, Gate8mDemo는3초마다 개폐 전환.
문짝 이동 거리 각각2m, 기본 속도1.25m/s. 완전 개폐 약1.6초.
정면 +Z 쪽 레일에서 좌우로 움직이며 벽 속으로 통과하지 않는다.
통로 하부 폭4m, 헤더 하단 Y=2.65m. 지면 레일 높이0.08m.
이동 물체의 실제 높이/폭과 지면 레일 통과 가능 여부는 프로젝트에서 확인해야 한다.
문짝 BoxCollider + 키네마틱 Rigidbody, FixedUpdate/MovePosition 사용.
고정 구조에는 부품별 정적 MeshCollider 사용. 루트 박스로 통로를 막지 않는다.
문 끼임 감지/자동 재개방/출입 인식/차량 이동/피해 처리/소리는 미포함.
상태등은 고정 색상 장식. NavMesh 갱신과 출입 제어 시스템은 별도 구현.
애니메이션은 런타임 스크립트 제어이며 Animator/AnimationClip 미포함.

Preview.png: 6종 카탈로그. GateOpen.png: 열린 게이트.
GateMotion.gif: 실제 메시 기반 개폐 모션. 모든 미리보기는 소프트웨어 렌더이며 Unity 캡처가 아니다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
