AI Defenders — 초원 헛간·풍차·건초 세트

구성: 총 6종, 2,720 삼각형
Barn: 빨간 헛간, 836 삼각형, 약 5.97×4.57×6.35 (X/Y/Z)
Windmill: 회전 날개 풍차, 672 삼각형, 닫힌 초기 자세 약 4.02×5.09×2.31
HayBaleSquare: 사각 건초, 132 삼각형, 약 1.15×0.70×0.75
HayBaleRound: 원형 건초, 564 삼각형, 약 1.31×1.31×1.42
HayStack: 쌓인 사각 건초 3개, 396 삼각형, 약 2.35×1.38×0.75
FeedTrough: 건초가 담긴 먹이통, 120 삼각형, 약 2.10×0.85×0.84

Unity 적용
1. Assets/AIDefendersMeadowFarm 폴더를 프로젝트 Assets에 복사합니다.
2. 컴파일 후 Tools > AI Defenders > Create Meadow Farm을 실행합니다.
3. Assets/AIDefendersMeadowFarmGenerated 폴더에 개별 프리팹과 재질·메시,
   MeadowFarmSample.prefab이 생성됩니다. 반복 실행 시 새 폴더에 생성합니다.
Built-in 및 URP용 재질을 지원합니다. HDRP는 재질 설정이 별도로 필요합니다.
이전에 제작한 초원·자연물·울타리 패키지에 의존하지 않고 단독 생성할 수 있습니다.

배치와 용도
1 Unity 단위=1m 기준, 모든 모델 바닥 접점은 Y=0, 위쪽은 +Y입니다.
헛간과 풍차의 전면은 +Z입니다. 초원 타일과 함께 배치하세요.
헛간은 지붕 포함 약 6×6.4이므로 4×4 타일 하나보다 큽니다.
모든 건물은 외관용입니다. 헛간은 내부에 들어갈 수 없는 닫힌 모델이며
문과 빗장은 고정 장식입니다. 문 개폐 동작은 포함하지 않습니다.
먹이통은 정적 장식이며 양의 먹기 AI나 자원 생산 시스템은 없습니다.

풍차 회전
풍차 루트의 MeadowWindmill 컴포넌트가 Rotor를 로컬 Z축으로 회전시킵니다.
namespace: AIDefenders.Assets.MeadowFarm
기본 playOnStart=true, degreesPerSecond=35입니다. 1회전은 약 10.29초입니다.
Play(): 재생 / Stop(): 정지 / SetSpeed(float): 속도 설정 / SetAngle(float): 각도 설정
음수 속도로 반대 방향 회전이 가능합니다. SetAngle은 자동 회전 중에도 호출 가능합니다.
플레이 모드에서 동작하는 C# 절차적 회전이며 AnimationClip/Animator는 없습니다.
날개는 시각 요소로서 충돌체가 없습니다. 바람이나 발전량을 계산하지 않습니다.

충돌
주요 정적 부품에는 로컬 경계 기준 근사 BoxCollider를 생성합니다.
곡면 풍차 몸통이나 원형 건초의 충돌 영역은 실제 곡면보다 넓을 수 있습니다.
풍차의 회전 날개와 허브에는 충돌체가 없습니다.
정밀 충돌, NavMeshObstacle, 파괴 기능, 지면 자동 정렬은 포함하지 않습니다.

포함 파일
Models: 정적 OBJ 6종과 공용 MTL (풍차 OBJ도 정지 상태)
Assets: Unity 에디터 프리팹 생성기, 메시 JSON, MeadowWindmill 런타임 스크립트
Preview.png: 모델별 크기를 각각 맞춘 카탈로그
SampleLayout.png: 실제 크기 기준 샘플 배치
WindmillMotion.gif: 동일 모델의 회전 시연 (대칭 날개 90도 구간 반복)
ModelInfo.json: 모델별 크기·삼각형 수
build_model.py, render_preview.py, render_layout.py: 재생성 소스
소스 재생성 환경: Python 3 / numpy / scipy / Pillow
텍스처 없이 색상 재질을 사용합니다. 텍스처용 UV는 없습니다.

샘플 프리팹은 소품만 포함합니다. 미리보기의 녹색 바닥은 크기 확인용이며
실제 씬에서는 별도 지면 또는 기존 초원 타일을 배치해야 합니다.
미리보기는 실제 메시를 소프트웨어 렌더링한 것으로 Unity 화면 캡처가 아닙니다.
유한 좌표, 유효 인덱스·삼각형 면적, 바닥 높이, 49개 각도에서의 회전 반경 및
날개 지면 여유를 확인했습니다. Unity 실행 환경이 없어 실제 에디터 컴파일,
플레이, 물리 충돌 검증은 하지 못했습니다.
