AI Defenders 사막 지형 세트 - 제작 순서 02

설치
Assets/AIDefendersDesertTerrain 폴더를 Unity 프로젝트 Assets에 복사합니다.
Tools > AI Defenders > Create Desert Terrain Kit 실행.
Assets/AIDefendersDesertTerrainGenerated 폴더에 모델 프리팹 7개와 예시 프리팹 1개 생성.

구성
SandTileFlat: 평평한 모래 바닥 10x10, 윗면 Y=0, 바닥 Y=-.25.
SandTileRipple: 가장자리 높이 0인 10x10 잔물결 바닥, 최대 높이 약 .05.
DuneLow: 낮은 언덕 약 7.2x5.6, 높이 .86.
DuneRidge: 긴 능선형 언덕 약 9x3.7, 높이 1.67.
RockBoulder: 낮고 넓은 바위, 높이 약 1.17.
RockSlab: 세로로 긴 바위, 높이 약 2.15.
RockCluster: 작은 바위 3개가 묶인 군집, 높이 약 1.13.
DesertTerrainExample: 30x30 바닥과 언덕/바위 배치 예시.
프리뷰는 개별 형태 확인을 위해 모델별 표시 배율이 다릅니다.

도로와 함께 사용
바닥은 스케일 1, X/Z 위치를 10유닛 단위로 배치하면 연결됩니다.
앞서 만든 Road Kit의 도로 루트를 같은 Y=0에 놓으면 노면 높이는 .08입니다.
도로 아래에는 SandTileFlat을 쓰세요. 언덕은 도로 바깥에 배치합니다.
예시의 중앙 X=0 주변은 도로 자리로 비워뒀으며 도로 자체는 이 ZIP에 포함되지 않습니다.
바위/언덕은 자유 회전/스케일 가능. 길을 가리거나 차량에 겹치지 않게 배치하세요.
바닥은 Unity Terrain 컴포넌트가 아닌 일반 메시입니다.
별도 텍스처 없이 면별 색상과 조명으로 표현한 로우폴리 스타일입니다.

Built-in/URP 재질 지원. HDRP는 별도 재질 설정 필요.
정적 MeshCollider 포함, Rigidbody/이동AI/NavMesh 설정은 포함하지 않습니다.
생성 메뉴 재실행 시 새 출력 폴더를 사용합니다.
OBJ 7개와 MTL, Unity 메시 데이터/생성기, Python 소스 포함.
Python 재생성은 numpy/Pillow 필요, Unity 사용에는 Python 불필요.
메시 인덱스/퇴화 삼각형/바닥 연결 높이/외향 면 검사 및 미리보기 확인.
Unity 내부 컴파일/실행은 미검증입니다.

다음 제작: 사막 주택 2종, 창고, 검문소.
