AI Defenders / 통신 안테나·회전 레이더 세트 3종

기존 군용 기지 모델과 어울리는 통신 철탑, 회전식 레이더, 접시형 안테나입니다.
실제 통신 장비의 기능을 구현한 것이 아닌 게임 배경 모델입니다.
각 칸의 미리보기는 확대/축소되어 상대 크기를 나타내지 않습니다.

[유니티 적용]
1. Assets/AIDefendersCommunications 폴더 전체를 프로젝트 Assets에 복사.
2. 컴파일 후 Tools > AI Defenders > Create Communications Set 실행.
3. Assets/AIDefendersCommunicationsGenerated 아래 각 폴더의 .prefab을 배치.
4. Play에서 RadarStation의 레이더 헤드가 회전합니다.
메뉴 재실행 시 별도 Generated 폴더를 생성해 기존 결과를 덮어쓰지 않습니다.
Built-in 및 URP 머티리얼을 생성하며 HDRP는 별도 재질 설정이 필요합니다.
기존 모델과 다른 클래스/폴더를 사용합니다.

[구성 / 원본 크기]
CommunicationsMast : 패널 안테나 3개, 철탑 보강재, 장비함 / 2,040 삼각형.
전체 약 폭 2.48 x 높이 6.99 x 깊이 1.75 Unity 단위.
RadarStation : 팔각 받침, 회전 베이스, 격자형 탐색 패널 / 996 삼각형.
정지 원본 약 폭 3.27 x 높이 3.72 x 깊이 2.38.
회전 중에는 정지 상태보다 앞뒤 차지하는 범위가 커집니다.
DishAntenna : 위로 25도 기울어진 접시, 수신부/지지대/기둥 / 1,420 삼각형.
전체 약 폭 2.43 x 높이 4.04 x 깊이 2.02.
3종 합계 4,456 삼각형입니다.
+Y 위쪽 / 기본 장비 정면은 +Z / 바닥 Y=0.
통신 철탑과 접시형 안테나는 고정 소품입니다. 회전 동작은 RadarStation만 포함합니다.

[레이더 회전]
RadarRotation 컴포넌트의 Is Rotating은 기본 켜짐입니다.
Degrees Per Second 기본 45도로 8초에 한 바퀴 회전합니다.
음수를 넣으면 반대 방향으로 돌고 0이면 정지합니다.
게임 코드에서 Play() / Stop()으로 회전을 재개/정지할 수 있습니다.
Stop()은 현재 방향을 유지합니다.
SetSpeed(float speed)로 회전 속도를 변경하고 SetAngle(float degrees)로 방향을 지정할 수 있습니다.
SetAngle() 자체는 자동 회전을 멈추지 않습니다. 고정 방향으로 두려면 Stop()도 호출하세요.
각도는 시작 시 RadarRotor의 로컬 회전을 기준으로 합니다.
RadarRotor의 피벗은 루트 기준 (0,1.95,0), 로컬 Y축 회전입니다.
Animator/AnimationClip 대신 런타임 Transform 회전 방식입니다.
회전 중 레이더 헤드 주변에 충분한 공간을 두세요.

[기능과 충돌]
레이더 감지, 적 추적, 피아식별, 미니맵, 통신/전파 시뮬레이션, 경보, 소리는 포함하지 않습니다.
실제 탐지 기능을 원하면 게임 탐지 코드와 이 모델의 회전 상태를 연결해야 합니다.
레이더/접시형 안테나에는 받침과 기둥 부근을 감싸는 단순 BoxCollider가 있습니다.
통신 철탑은 각 기초·다리·장비함에 BoxCollider를 나눠 배치합니다.
철탑 다리 충돌체는 기울어진 메시의 축 정렬 박스 근사치입니다.
회전 헤드, 접시, 패널, 가느다란 보강재의 정밀 충돌은 포함하지 않습니다.
Rigidbody, 올라가기/등반, 파손, 전원/보급 기능, NavMeshObstacle을 자동 설정하지 않습니다.
프로젝트 이동 방식에 맞춰 길찾기 장애물 및 충돌체를 조정하세요.

[파일]
Assets/.../Editor : 메시 JSON 및 프리팹/머티리얼 생성기
Assets/.../Runtime : RadarRotation.cs
Models/*.obj + Communications.mtl : 개별 정지 메시. OBJ에는 계층이나 회전 스크립트 없음.
Preview.png : 실제 모델 3종의 정지 소프트웨어 렌더링
RadarPreview.gif / .png : 실제 메시의 360도 회전 미리보기 (유니티 실행 녹화 아님)
ModelInfo.json : 정지 상태 크기, 피벗, 삼각형 수
Python 생성/렌더링 원본 : numpy, Pillow 필요.
단색 재질과 플랫 노멀을 사용하며 텍스처용 UV는 없습니다.

[검증 범위]
부품 이름, 면적, 인덱스, JSON/원본 좌표와 회전 피벗 연결을 검사하고
실제 메시의 정지/회전 미리보기를 확인했습니다.
유니티가 없는 환경이므로 유니티 컴파일/플레이 및 물리 테스트는 수행하지 못했습니다.
