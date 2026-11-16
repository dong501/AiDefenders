AI Defenders 도시 인도/연석/화단 세트 - 제작 순서 05

설치
Assets/AIDefendersCitySidewalk 폴더를 Unity 프로젝트 Assets에 복사.
Tools > AI Defenders > Create City Sidewalk Kit 실행.
Assets/AIDefendersCitySidewalkGenerated에 모델 7개와 예시 프리팹 1개 생성.

구성
SidewalkStraight: 양쪽 직선 인도, 길이 10 / 각 폭 2유닛.
SidewalkCurve: 기존 Road_Curve와 맞는 90도 바깥/안쪽 인도.
SidewalkT: 기존 T자 교차로용 인도.
SidewalkCross: 기존 십자 교차로용 네 모서리 인도.
SidewalkBlock: 자유 배치용 2x2 포장 블록.
PlanterSquare: 1.15x1.15 사각 화단.
PlanterLong: 1.15x2.7 긴 화단.
CitySidewalkExample: 이전 Road Kit의 예시와 같은 도로 배치에 맞춘 인도 예시.
도로/주변 건물/바닥 자체는 포함하지 않습니다.

도로와 연결
도로와 해당 인도 프리팹의 루트 위치/회전을 동일하게 맞춥니다.
X/Z는 10유닛 간격, Y회전은 90도 단위, 스케일 1.
City_Crosswalk 도로에는 SidewalkStraight를 사용합니다.
노면 높이 .08, 인도 높이 .20, 줄눈 .202, 연석 높이 .24유닛.
화단을 인도 위에 놓으면 화단 루트 Y=.202로 설정합니다.
교차로 인도는 차도 공간이 비어 있는 것이 정상입니다.
곡선 인도는 Road_Curve와 같은 중심 (-5,-5), 같은 방향을 사용합니다.
연석은 차도 경계에 걸쳐 놓입니다. 현재 턱 낮춤/휠체어 경사로는 포함하지 않습니다.
직선 인도는 양쪽이 하나의 프리팹입니다. 편집 시 좌우 메시를 선택해 조정할 수 있습니다.

+Y 위, 일반 메시 기반. Built-in/URP 지원, HDRP 별도 재질 설정.
정적 MeshCollider 포함. 보행AI/NavMesh/자동스냅/교통신호 로직은 포함하지 않습니다.
기본 단색 재질, 줄눈과 관목은 실제 메시입니다.
OBJ 7개, 공용 MTL, Unity 생성기와 메시 데이터, Python 소스, 프리뷰 포함.
Python 재생성: numpy/Pillow 필요. Unity 사용: Python 불필요.
프리뷰의 모델별 표시 배율은 다릅니다.
메시 인덱스/퇴화 면과 도로 폭 대응 검사, 프리뷰 확인 완료.
Unity 내부 컴파일/실행은 미검증입니다.

다음 제작: 도시 상가·주택·빌딩.
