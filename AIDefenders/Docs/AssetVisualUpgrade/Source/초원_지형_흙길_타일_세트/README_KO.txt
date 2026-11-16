AI Defenders — 초원 지형·흙길 타일 세트

구성
12종 정적 메시 타일, OBJ/MTL, Unity 에디터 프리팹 생성기와 메시 데이터,
5×5 배치 예시, 이동 경유점 12개, 실제 메시 렌더 미리보기, 재생성용 Python 소스.
타일당 642 삼각형, 전체 7,704 삼각형. 별도 텍스처 없이 색상 재질을 사용합니다.
Unity Terrain 오브젝트가 아니라 MeshFilter/MeshRenderer 기반 모델입니다.

Unity 적용
1. ZIP을 풀고 Assets/AIDefendersGrasslandTiles 폴더를 프로젝트 Assets에 복사합니다.
2. 컴파일이 끝나면 Tools > AI Defenders > Create Grassland Tiles를 실행합니다.
3. Assets/AIDefendersGrasslandTilesGenerated 폴더에 타일 12개와
   GrasslandSampleLayout.prefab이 생성됩니다. 반복 실행하면 새 폴더를 만듭니다.
4. 프리팹을 씬에 배치합니다. Built-in과 URP용 재질을 지원하며 HDRP는 별도 설정이 필요합니다.

격자와 연결 규칙
타일 크기 X/Z 각각 4 Unity 단위, 중심 원점 기준 -2~+2입니다.
평지 윗면 Y=0, 밑면 Y=-0.35입니다. 4단위 위치 스냅, Y축 90도 회전으로 연결합니다.
길 폭은 1.5입니다. N=+Z, E=+X, S=-Z, W=-X이며 아래 방향은 회전 전 로컬 기준입니다.

GrassA / GrassB: 평지 잔디 2종
PathStraight: 직선, N·S
PathCurve: 90도 곡선, W·S
PathTJunction: T자, W·E·S
PathCross: 십자, N·E·S·W
PathEnd: 막다른 길, S
GrassHill: 중심 높이 0.72의 낮은 언덕, 경계 높이 0
GrassRamp: S 높이 0에서 N 높이 0.8까지 경사
GrassRaised: 전체 윗면 높이 0.8
PathRamp: 경사 위 직선 길, N·S
PathRaisedStraight: 윗면 높이 0.8의 직선 길, N·S

경사는 낮은 끝을 평지에, 높은 끝을 높은 타일에 연결합니다.
경사의 E/W 가장자리는 기울어져 있으므로 평지에 바로 붙이면 높이 차이가 생깁니다.
고지대 외곽은 흙 벽으로 표현합니다. 서로 높이가 맞는 가장자리를 연결하세요.
잔디와 흙길은 작은 면의 색상 변화로 표현합니다. 곡선 외곽은 격자 형태입니다.

샘플 배치
20×20 범위의 5×5 지면과 두 번 꺾이는 연속 흙길을 포함합니다.
ExampleWaypoints 아래 Route_00~Route_11이 이동 순서입니다.
샘플 루트 기준 입구(-10,0,0), 출구(10,0,-8)입니다.
경유점은 빈 Transform이며 적 이동, 스폰, NavMesh, 포탑 설치 코드는 포함하지 않습니다.
길 색상 자체에는 이동 판정 기능이 없습니다. 기존 이동 시스템에 경유점을 연결하세요.

충돌
각 타일에는 지형 메시와 같은 형상의 비볼록 MeshCollider를 생성합니다.
정적 지형용이며 Rigidbody가 없습니다. 움직이는 물체로 사용할 경우 충돌 설정을 별도로 조정하세요.

미리보기 및 검증
Preview.png는 12종 카탈로그, SampleLayout.png는 샘플 배치의 실제 메시 소프트웨어 렌더입니다.
Unity 화면 캡처가 아닙니다. 모델의 유한 좌표, 삼각형 면적, 바깥쪽 면 방향,
길 연결 폭, 경사 높이, 샘플의 인접 경계 40곳과 경유점 위치를 확인했습니다.
Unity 실행 환경이 없어 실제 에디터 컴파일 및 플레이 검증은 하지 못했습니다.
나무·덤불·바위·풀, 캐릭터, 포탑, 늑대는 이 패키지에 포함되지 않습니다.

소스 재생성
Python 3와 numpy, Pillow가 필요합니다.
build_model.py → render_preview.py → render_layout.py 순서로 실행합니다.
