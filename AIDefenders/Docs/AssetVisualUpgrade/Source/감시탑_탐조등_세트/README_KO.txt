AI Defenders / 감시탑·회전 탐조등 세트

기존 막사, 격납고, 검문소와 어울리는 로우폴리 감시 시설입니다.
Watchtower(탐조등이 달린 감시탑)와 GroundSearchlight(독립 기둥형 탐조등)를 생성합니다.
미리보기는 각 모델을 확대/축소한 것으로 상대 크기를 나타내지 않습니다.

[유니티 적용]
1. Assets/AIDefendersWatchtower 폴더 전체를 프로젝트의 Assets로 복사.
2. 컴파일 완료 후 Tools > AI Defenders > Create Watchtower Set 실행.
3. Assets/AIDefendersWatchtowerGenerated 아래 각 폴더의 프리팹을 씬에 배치.
4. Play를 누르면 각 탐조등이 좌우로 탐색하며 Spot Light 방향도 함께 바뀝니다.
재실행 시 새 Generated 폴더가 생성되어 기존 결과를 덮어쓰지 않습니다.
Built-in 및 URP용 재질을 생성합니다. HDRP는 별도 재질 설정이 필요합니다.

[구성]
Watchtower : 감시탑 + 지붕형 탐조등 / 2,112 삼각형.
4개 지지 다리, X자 보강대, 사다리, 관측 발판/난간, 지붕, 회전 탐조등.
본체 폭 약 3.18, 기본 메시 높이 약 6.57 Unity 단위.
관측 발판 상단은 Y=3.60이며 내부에 경비병을 배치할 수 있는 빈 공간을 두었습니다.
군인 캐릭터와 사다리 오르기/탑승 동작은 포함하지 않습니다.
사다리는 외형이며 인터랙션/등반 기능이나 개별 발판 충돌체가 없습니다.

GroundSearchlight : 콘크리트 받침 + 기둥 + 제어함 + 탐조등 / 884 삼각형.
받침 폭 1.05, 기본 메시 높이 약 3.42 Unity 단위.
두 모델 합계 2,996 삼각형입니다. 탐조등 회전/기울기에 따라 외곽 크기는 변합니다.
+Y 위쪽 / +Z 탐조등의 기본 정면 / 지면 Y=0.

[탐조등 제어]
SearchlightSweep 컴포넌트:
Auto Sweep : 기본 켜짐. 좌우를 부드럽게 반복 탐색합니다.
Sweep Degrees : 중심에서 좌/우 최대 회전각. 기본 ±65도.
Cycle Seconds : 한 번의 전체 왕복 주기. 기본 12초.
Tilt Degrees : 아래로 비추는 각도. 기본 20도.
Manual Yaw : Auto Sweep을 끄면 사용하는 좌우 방향.
Light On : 실제 Spot Light 활성 여부. 렌즈 재질의 밝기는 별도입니다.
게임 코드에서 SetAim(yawDegrees, downwardTilt)를 호출하면 자동 탐색이 꺼지고 지정 방향으로 고정됩니다.
SetLight(true/false)로 조명을 켜거나 끄고 ResumeSweep()으로 자동 탐색을 재개할 수 있습니다.
좌우 회전은 SearchlightYaw의 로컬 Y축, 상하 회전은 SearchlightPitch의 로컬 X축입니다.
Tilt Degrees가 양수면 아래로, 음수면 위로 비춥니다.
Animator/AnimationClip 대신 런타임 스크립트 방식입니다.
적 자동 탐지/추적, 피아식별, 경보, 피해, 소리는 포함하지 않습니다.
SetAim은 순간 방향 설정입니다. 별도의 목표 추적/회전 속도 보간은 없습니다.

[실제 조명]
각 SearchlightPitch/SpotLight에 Unity Light(Type=Spot)를 연결했습니다.
초기 Range=22, Spot Angle=38, Intensity=3, 색상은 따뜻한 흰색입니다.
실제 밝기/거리감은 프로젝트의 렌더 파이프라인, 노출, 환경 조명에 따라 조절하세요.
그림자는 비용을 줄이기 위해 기본 None입니다. 필요하면 직접 설정하세요.
렌즈에는 Emission 재질이 있지만 실제 표면 조명은 Spot Light가 담당합니다.
공중에서 빛줄기가 보이는 볼류메트릭 빔/안개/반투명 원뿔은 포함하지 않습니다.
렌즈 빛 번짐은 Bloom, 공중 빛줄기는 프로젝트에 맞는 별도 효과가 필요합니다.
SweepPreview.gif는 실제 메시 회전만 보여주며 Unity 조명 렌더링을 시연하는 영상은 아닙니다.

[배치와 충돌]
탑의 기초, 다리, 발판, 낮은 벽, 지붕과 독립형 받침/기둥에 단순 BoxCollider를 둡니다.
탑 전체를 막는 거대한 하나의 박스는 사용하지 않습니다.
보강대/난간/사다리/탐조등 머리의 정밀 충돌은 포함하지 않습니다.
사다리는 장식이므로 군인을 탑 위에 둘 때 루트 위치를 발판 높이 Y=3.60에 직접 설정하세요.
NavMesh, 등반, 낙하 방지, 경비 AI는 별도 연결해야 합니다.
특히 난간의 빈 공간이나 외형이 캐릭터의 추락을 막는다고 가정하지 마세요.
지지 다리 충돌체는 기울어진 메시 외곽의 축 정렬 박스 근사치입니다.
Rigidbody는 추가하지 않으며 탐조등 회전부에는 물리 충돌을 사용하지 않습니다.

[파일]
Assets/.../Editor : 데이터 JSON, 프리팹/재질/조명 생성기
Assets/.../Runtime : SearchlightSweep.cs
Models/*.obj + Watchtower.mtl : 개별 정지 원본 메시 (OBJ에는 피벗 계층/조명/애니메이션 없음)
Preview.png : 실제 메시의 정지 미리보기
SweepPreview.gif / .png : 실제 메시의 탐조등 회전 소프트웨어 미리보기
미리보기는 Unity 실행 녹화가 아닙니다.
ModelInfo.json : 크기/피벗/삼각형 수. 크기는 회전 전 원본 메시 기준입니다.
Python 생성/렌더링 원본 : numpy/Pillow 필요.
단색 재질과 플랫 노멀을 사용하며 텍스처용 UV는 없습니다.

[확인 범위]
면적, 부품 이름, 인덱스, JSON/원본 좌표, 회전 피벗 계층과 미리보기를 확인했습니다.
Unity가 없는 환경이라 Unity 컴파일/플레이, 실제 조명, 물리 테스트는 수행하지 못했습니다.
