AI Defenders / 정찰 드론 + 로터·이착륙 시연

기존 군용 모델과 어울리는 회색/붉은 포인트의 로우폴리 4로터 드론입니다.
카메라, 고정 짐벌 장식, 상부 통풍구, 안테나, 스키드 착륙 다리를 포함합니다.
특정 실기체 복제 모델이 아니며 아군/적군 판정은 포함하지 않습니다.
1,756 삼각형 / 정지 외곽 약 폭 2.62 x 높이 1.19 x 깊이 2.08 Unity 단위.
회전 중 전체 폭/깊이는 각각 약 2.70/2.60까지 늘어나므로 충분한 공간을 두세요.
+Z 전방 / +Y 위쪽 / 단품 착륙 다리 바닥 Y=0.

[유니티 적용]
1. Assets/AIDefendersReconDrone 폴더 전체를 프로젝트 Assets에 복사.
2. 컴파일 후 Tools > AI Defenders > Create Recon Drone 실행.
3. Assets/AIDefendersReconDroneGenerated/ReconDrone/ReconDrone.prefab을 씬에 배치.
4. Play에서 기본 자동 이륙·공중 대기·착륙 시연과 로터 회전이 재생됩니다.
메뉴 재실행 시 새 Generated 폴더가 생겨 기존 결과를 덮어쓰지 않습니다.
Built-in 및 URP 재질을 생성하며 HDRP는 별도 재질 설정이 필요합니다.
헬기 등 기존 모델과 다른 클래스/폴더를 사용합니다.

[포함 동작]
ReconDroneMotion에서 본체 위치와 프로펠러 Transform을 움직이는 스크립트 방식입니다.
Animator/AnimationClip은 아닙니다.
- 4개 로터 회전, 대각선 로터끼리 같은 방향 / 인접 로터끼리 반대 방향
- 수직 이륙 → 공중 대기 → 착륙 반복
- 공중에서 작은 상하 흔들림
Rotor Running 기본 켜짐, Rotor Degrees Per Second 기본 750도/초.
Hover Height 기본 1.6은 시작 지면에서의 추가 고도입니다.
Travel Seconds 기본 2초, Hover Bob 기본 .025입니다.
Auto Demo의 기본 10초 주기:
0~2초 지상 → 2~4초 상승 → 4~7초 공중 대기 → 7~9초 하강 → 9~10초 지상.
자동 시연의 목표 전환 시각은 고정이므로 Travel Seconds를 크게 바꾸면 주기와 맞지 않을 수 있습니다.
지상에서도 Rotor Running을 켜두면 프로펠러가 회전합니다.
고속 프로펠러는 프레임률에 따라 느리게/역으로 보일 수 있으며 별도 블러 디스크는 없습니다.

[게임 코드 연결]
TakeOff() : Auto Demo를 끄고 Hover Height까지 상승.
Land() : Auto Demo를 끄고 처음 기준 지면으로 하강.
SetRotors(true/false) : 프로펠러 회전 켜기/끄기. 관성/출력 물리 없이 즉시 반영됩니다.
UseExternalMovement() : 자동 고도 제어를 끄고 기존 비행/이동 코드로 위치를 제어할 때 사용.
ResetGroundReference() : 착륙한 드론을 옮긴 후 현재 Y를 새 지면 기준으로 지정.
IsAirborne은 자체 시연 고도만 반영하며 외부 이동의 실제 비행 여부를 감지하지 않습니다.
외부 경로 이동이 필요하면 UseExternalMovement()를 호출한 후 루트 Transform을 움직이세요.
Drive Altitude를 켜두면 시작 시 저장한 지면을 기준으로 Y를 매 프레임 제어하므로
외부 코드가 고도를 움직일 때는 꺼야 합니다.
SetRotors(false)만으로 자동 착륙하거나 추락하지는 않습니다.
경사면/움직이는 착륙 지점/장애물을 자동 감지하지 않습니다.

[카메라와 충돌]
카메라는 고정된 장식 메시이며 실제 Unity Camera를 추가하지 않습니다.
카메라 짐벌 회전, 적 탐지/추적, 피아식별, 영상 처리, 정찰 범위 표시, 사격 기능은 없습니다.
충돌체는 중앙 동체 부근의 단순 BoxCollider이며 로터/다리/카메라의 정밀 충돌은 없습니다.
Rigidbody, 양력/공기역학, 실제 비행 제어, 경로 탐색, 장애물 회피,
충돌에 의한 정지, 피해/체력, 소리/먼지는 포함하지 않습니다.
Body 아래에 본체 메시와 Rotor0~3이 배치되며 기체 흔들림을 함께 따라갑니다.
각 로터는 해당 피벗의 로컬 Y축으로 회전합니다.
렌즈와 후방 표시등은 Emission 재질이며 실제 광원/점멸 동작은 없습니다.
루트 스케일은 균등하게 사용하는 것을 권장합니다.

[파일]
Assets/.../Editor : 메시 JSON 및 프리팹/재질 생성기
Assets/.../Runtime : ReconDroneMotion.cs
Models/ReconDrone.obj + .mtl : 정지 원본. OBJ에는 회전 피벗 계층/스크립트 없음.
Preview.png : 실제 모델 앞/뒤 미리보기
FlightPreview.gif / .png : 실제 메시의 수직 이착륙·로터 회전 소프트웨어 미리보기
위 영상은 유니티 실행 녹화가 아닙니다.
ModelInfo.json : 정지 크기, 로터 피벗, 삼각형 수
Python 생성/렌더링 원본 : numpy/Pillow 필요.
단색 재질/플랫 노멀을 사용하며 텍스처용 UV는 없습니다.

[검증 범위]
부품 이름, 면적, 인덱스, JSON/원본 좌표, 4로터 및 컴포넌트 연결을 검사하고
실제 메시의 정지/이착륙 미리보기를 확인했습니다.
유니티가 없는 환경이라 유니티 컴파일/플레이, 실제 비행/충돌 테스트는 수행하지 못했습니다.
