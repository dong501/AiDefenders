AI Defenders / 군용 발전기·전력 설비 세트 4종

기존 군용 기지와 어울리는 발전기, 배전함, 케이블 릴, 고정식 연료탱크입니다.
개별 프리팹 4종을 생성하며 발전기에만 냉각팬 회전 시연을 포함합니다.
미리보기는 각 칸을 확대/축소한 것으로 상대 크기를 나타내지 않습니다.

[유니티 적용]
1. Assets/AIDefendersPowerFacilities 폴더 전체를 프로젝트 Assets로 복사.
2. 컴파일 후 Tools > AI Defenders > Create Power Facilities 실행.
3. Assets/AIDefendersPowerFacilitiesGenerated 아래 각 폴더의 .prefab을 씬에 배치.
4. Play에서 FieldGenerator의 냉각팬이 돌아갑니다.
메뉴 재실행 시 별도 Generated 폴더를 만들어 기존 결과를 덮어쓰지 않습니다.
Built-in 및 URP 머티리얼을 생성하며 HDRP는 별도 재질 설정이 필요합니다.
기존 모델과 다른 클래스 및 폴더를 사용합니다.

[구성 / 삼각형 수 / 원본 크기]
FieldGenerator : 배기구, 운반 고리, 제어판, 측면 통풍구, 냉각팬 / 1,628.
약 폭 3.84 x 높이 2.29 x 깊이 1.93 Unity 단위.
PowerCabinet : 계기판, 손잡이, 통풍구가 있는 배전함 / 544.
약 폭 1.16 x 높이 1.98 x 깊이 .84.
CableReel : 목제 플랜지, 케이블 코일, 크래들 받침 / 1,680.
약 폭 1.26 x 높이 1.42 x 깊이 1.46 (늘어진 케이블 포함).
FuelStorageTank : 콘크리트 바닥, 받침, 원통형 탱크, 주입구, 전면 밸브 / 1,024.
약 폭 2.32 x 높이 2.36 x 깊이 4.30.
4종 전체 합계 4,876 삼각형입니다.
+Y 위쪽 / 기본 장비 정면 +Z / 지면 Y=0.

[발전기 팬 제어]
FieldGenerator의 FacilityFan 컴포넌트에서 Is Rotating 기본 켜짐.
Degrees Per Second 기본 500도/초, 음수를 넣으면 반대 방향입니다.
Stop()은 현재 방향에서 정지, Play()는 재개합니다.
SetSpeed(float speed)로 회전 속도를 변경할 수 있습니다.
SetAngle(float degrees)로 현재 방향을 설정할 수 있습니다. 고정하려면 Stop()도 호출하세요.
각도는 시작 시 팬의 로컬 회전을 기준으로 합니다.
회전 피벗 CoolingFan=(X -.74, Y 1.02, Z .962), 로컬 Z축으로 회전합니다.
보호망은 고정되어 있고 내부 팬 날개/허브만 회전합니다.
Animator/AnimationClip 대신 런타임 Transform 회전 스크립트 방식입니다.
고속 팬은 프레임률에 따라 느리게/역으로 보일 수 있으며 모션 블러는 포함하지 않습니다.
표시창은 Emission 재질이며 실제 광원이나 전원 상태를 표시하는 UI는 아닙니다.
팬 회전과 표시창 발광은 별도이므로 Stop()만으로 표시창이 꺼지지는 않습니다.

[게임 기능·충돌]
전력 생산/배분, 연료량/소모, 보급, 건물 연결, 고장/파손, 소리/연기/불꽃은 포함하지 않습니다.
버튼/계기/밸브/문/뚜껑은 고정 장식입니다.
케이블 릴의 풀기/감기나 탱크 급유 동작은 없습니다.
게임에서 발전시설로 쓸 때는 자원/전력 상태와 상호작용을 별도 코드에 연결하세요.
각 프리팹에는 전체 외곽을 감싸는 단순 BoxCollider가 있습니다.
팬은 시각적 회전이며 독립 충돌체를 추가하지 않습니다.
릴과 탱크의 빈 공간/둥근 외곽은 박스 충돌체와 정확히 일치하지 않습니다.
Rigidbody, 운반 물리, NavMeshObstacle 또는 길찾기 장애물을 자동 설정하지 않습니다.
배경 장식만 필요하면 Collider를 끄거나 프로젝트에 맞춰 수정하세요.

[파일]
Assets/.../Editor : 메시 JSON 및 프리팹/재질 생성기
Assets/.../Runtime : FacilityFan.cs
Models/*.obj + PowerFacilities.mtl : 개별 정지 메시. OBJ에는 팬 피벗 계층/애니메이션 없음.
Preview.png : 설비 4종의 실제 메시 미리보기
FanPreview.gif / .png : 실제 메시 냉각팬의 소프트웨어 회전 미리보기 (유니티 녹화 아님)
ModelInfo.json : 원본 크기/피벗/삼각형 수
Python 생성/렌더링 원본 : numpy/Pillow 필요.
단색 재질과 플랫 노멀을 사용하며 텍스처용 UV는 없습니다.

[검증 범위]
부품 이름, 면적, 인덱스, JSON/원본 정합성, 팬 피벗 및 충돌체 기준 좌표를 검사하고
실제 설비와 팬 회전 미리보기를 확인했습니다.
유니티가 없는 환경이라 유니티 컴파일/플레이 및 물리 테스트는 수행하지 못했습니다.
