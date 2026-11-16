AI Defenders 방어 기지 - 게임 목표 모델 01

Assets/AIDefendersDefenseBase 폴더를 Unity 프로젝트 Assets에 복사.
Tools > AI Defenders > Create Defense Base Kit 실행.
Assets/AIDefendersDefenseBaseGenerated 폴더에 DefenseHQ, GoalMarker, DefenseBaseExample 프리팹 생성.

DefenseHQ: 남색/금색 장갑 기지, 중앙 에너지 코어, 출입문, 진입 경사로.
전체 폭 6.6, 깊이 6.4, 높이 3.8유닛. +Z 정면, 루트 Y=0이 바닥.
GoalMarker: 별도로 배치하는 장식용 목표 바닥 표시. 게임 판정/트리거 없음.
GoalPoint: 기지 앞 목표 위치, 기지 로컬 좌표 (0,.08,3.8).
HealthBarAnchor: 체력바 배치 기준점, 기지 로컬 좌표 (0,4.25,0).
CoreEnergy / DoorStatus / BaseStatusL / BaseStatusR: 별도 발광 메시.
피격 시 색상 변경/점멸/코어 파괴 연출은 별도 코드로 연결해야 합니다.

배치
기지는 10유닛 격자의 빈 대지 위에 배치할 수 있지만 도로 위를 막지 않게 배치합니다.
기지 루트가 (0,0,0)이면 앞 경사로 끝은 Z=3.6, Y=.08입니다.
길이 10의 직선 도로 중심을 (0,0,8.6)에 두면 도로 끝과 경사로 끝이 맞닿습니다.
도로 폭은 6, 경사로 폭은 2.1유닛입니다. 차량을 기지 안에 진입시키는 구조가 아닙니다.
적은 GoalPoint 근처에 도착했을 때 체력을 감소시키고 제거하는 방식으로 연결할 수 있습니다.
이 동작, 체력 시스템, 트리거, UI, 승패 판정, 실제 내부 공간은 제공하지 않습니다.
문은 닫힌 외관 모델입니다. 개폐 기능 없음.
바닥 표시는 노면 위에 배치할 때 루트 Y=.08로 높여주세요.
예시 프리팹에는 도로/지형이 포함되지 않습니다.

Built-in/URP 재질 지원, HDRP 별도 설정.
발광은 재질 표현이며 실제 Light/Bloom 설정은 포함하지 않습니다.
기초/장갑 벽/지붕/경사로에 정적 MeshCollider 적용.
OBJ 2개, 공용 MTL, Unity 생성기/데이터, Python 소스, 프리뷰 포함.
Python 재생성에 numpy/Pillow 필요. Unity 사용에는 Python 불필요.
메시 인덱스/퇴화 면/유한 좌표 검사 및 프리뷰 확인. Unity 내부 실행 미검증.

제작 순서
01 방어 기지 완료
02 스폰 게이트
03 민간 차량: 버스/구급차/소방차
04 파괴된 차량/포탑 잔해
02~04는 이 패키지에 포함하지 않습니다.
