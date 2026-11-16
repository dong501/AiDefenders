AI Defenders / 수송헬기·착륙장 + 로터·이착륙 시연

기존 군용 기지와 어울리는 로우폴리 수송헬기, H 표시 착륙장 세트입니다.
특정 실차를 복제한 모델이 아니며 비행 물리 시뮬레이터는 아닙니다.
헬기와 착륙장 단품 2개 및 조립형 HelicopterOnHelipad 프리팹을 생성합니다.
정지 미리보기의 두 칸은 각각 확대/축소되어 상대 크기를 나타내지 않습니다.

[유니티 적용]
1. Assets/AIDefendersHelicopter 폴더 전체를 프로젝트 Assets로 복사.
2. 컴파일 후 Tools > AI Defenders > Create Helicopter Set 실행.
3. Assets/AIDefendersHelicopterGenerated/HelicopterOnHelipad.prefab을 씬에 배치.
4. Play에서 로터 회전과 수직 이륙·공중 대기·착륙을 반복합니다.
단품은 TransportHelicopter/TransportHelicopter.prefab, Helipad/Helipad.prefab에 있습니다.
생성 메뉴 재실행 시 별도 Generated 폴더를 만들어 기존 결과를 덮어쓰지 않습니다.
Built-in 및 URP용 재질을 생성하며 HDRP는 별도 재질 설정이 필요합니다.
이전 모델과 별도 클래스/폴더를 사용합니다.

[모델 구성]
TransportHelicopter / 1,792 삼각형.
동체/조종석 유리, 측면 화물문과 창문, 엔진 흡배기 장식, 꼬리 붐/안정판,
착륙 스키드, 메인 로터 4엽, 꼬리 로터 4엽.
정지 원본 외곽 약 폭 8.87 x 높이 3.29 x 길이 10.15 Unity 단위.
메인 로터가 도는 전체 폭은 약 9.2이므로 주변에 충분한 공간을 두세요.
꼬리 로터와 기체까지 포함한 전체 길이/형태를 고려해 배치하세요.
Helipad / 1,020 삼각형. 폭 12.4 x 깊이 12.4, 기초 상단 Y=.18.
바닥 표면과 H/원형/외곽 표시, 정적인 유도등 메시를 포함합니다.
표시까지 포함한 중앙 상단 높이는 약 .207, 모서리 유도등 높이는 .33입니다.
조립형 헬기는 착륙장 표시 위에 겹치지 않도록 루트 Y=.206에 놓입니다.
메시 전체 합계 2,812 삼각형입니다.
+Y 위쪽 / +Z 헬기 전방 / 단품 헬기 스키드 바닥 Y=0.
조종사, 실내, 문 열림, 무장/사격, 승하차, 연기/먼지, 엔진음은 포함하지 않습니다.
색상 항법등/유도등은 Emission 재질이며 실제 Point Light나 점멸 동작은 아닙니다.

[로터와 시연]
HelicopterMotion의 Rotor Running은 기본 켜짐입니다.
메인 로터 650도/초, 꼬리 로터 960도/초로 기본 설정했습니다.
음수 속도를 설정하면 반대 방향으로 회전합니다.
Auto Demo 기본 켜짐. 10초 주기에서 기본값 기준:
0~2초 지상 대기 → 2~4초 상승 → 4~7초 공중 대기 → 7~9초 하강 → 9~10초 지상 대기.
Hover Height 기본 2.2는 시작 지면에 대한 추가 고도입니다.
Travel Seconds 기본 2는 목표 고도로 바뀌는 시간입니다.
Auto Demo의 목표 전환 시각은 고정이므로 Travel Seconds를 크게 바꾸면 시연 주기와 맞지 않을 수 있습니다.
Hover Bob은 공중에서 보이는 작은 기체 흔들림입니다.
기체가 지상에 있어도 Rotor Running을 켜두면 로터는 계속 회전합니다.
고속 로터는 프레임률에 따라 느리게 또는 역으로 보이는 샘플링 현상이 생길 수 있습니다.
별도의 블러 디스크/모션 블러 메시를 포함하지 않습니다.

[게임 코드 연결]
TakeOff() : Auto Demo를 끄고 현재 시연 고도에서 Hover Height로 상승.
Land() : Auto Demo를 끄고 처음 기준 지면 높이로 하강.
SetRotors(true/false) : 로터 회전 켜기/끄기. 즉시 반영되며 관성/출력 물리는 없습니다.
UseExternalMovement() : 자동 고도 제어와 시연을 끄고 위치 제어를 외부 이동 시스템에 넘깁니다.
ResetGroundReference() : 착륙한 기체를 다른 위치로 옮긴 후 현재 Y를 새 지면 기준으로 지정.
IsAirborne은 자체 고도 시연에서 올라간 높이만 나타내며 외부 비행 상태를 자동 감지하지 않습니다.
외부 비행 경로가 있다면 UseExternalMovement()를 호출한 뒤 루트 Transform을 제어하세요.
기본 Drive Altitude를 켜두면 시작 시 저장한 지면 높이를 기준으로 Y를 매 프레임 제어합니다.
외부 코드가 고도를 제어할 때는 Drive Altitude를 꺼야 서로 충돌하지 않습니다.
SetRotors(false)만으로 헬기가 자동 착륙하거나 추락하지는 않습니다.
TakeOff/Land는 World Y 기준이며 자동 지면 추적/경사면 착륙은 없습니다.

[애니메이션·충돌]
Animator/AnimationClip 대신 Transform 회전·이동을 사용하는 런타임 스크립트입니다.
메인 로터는 Body/MainRotor의 로컬 Y축, 꼬리 로터는 Body/TailRotor의 로컬 X축으로 회전합니다.
기체 흔들림은 Body 아래 모든 메시/로터에 함께 적용됩니다.
헬기의 충돌체는 동체 부근의 단순 BoxCollider이며 스키드/꼬리/로터의 정밀 충돌을 포함하지 않습니다.
착륙장의 충돌체는 기초 상단 Y=.18까지의 박스이며 도색/유도등은 충돌하지 않습니다.
Rigidbody, 양력/공기역학, 비행 경로, 장애물 회피, 충돌에 의한 정지, 피해/체력/피아식별은 없습니다.
기본 이착륙은 시연이므로 게임에서 쓰려면 이동, 충돌, 임무/유닛 상태를 별도 연결하세요.
루트 스케일은 균등하게 사용하는 것을 권장합니다.

[파일]
Assets/.../Editor : JSON 메시 데이터 및 프리팹/재질 생성기
Assets/.../Runtime : HelicopterMotion.cs
Models/*.obj + Helicopter.mtl : 개별 정지 원본. OBJ에는 피벗 계층/스크립트 애니메이션 없음.
Preview.png : 헬기와 착륙장 실제 메시 정지 미리보기
FlightPreview.gif / .png : 실제 메시의 이륙·공중 대기·착륙 소프트웨어 미리보기
영상은 유니티 실행 녹화가 아닙니다.
ModelInfo.json : 정지 외곽 크기, 로터 피벗, 삼각형 수
Python 생성/렌더링 원본 : numpy, Pillow 필요.
단색 재질/플랫 노멀을 사용하며 텍스처용 UV는 없습니다.

[확인 범위]
면적, 부품 이름, 인덱스, JSON/원본 좌표와 로터 피벗, 착륙장 배치 범위를 검사하고
실제 모델 정지 및 이착륙/로터 회전 미리보기를 확인했습니다.
유니티가 없는 환경이라 유니티 컴파일/플레이, 비행/충돌 테스트는 수행하지 못했습니다.
