AI Defenders 도시 시설 세트 - 제작 순서 07

설치
Assets/AIDefendersCityFacilities 폴더를 Unity 프로젝트 Assets에 복사합니다.
Tools > AI Defenders > Create City Facilities Kit 실행.
Assets/AIDefendersCityFacilitiesGenerated에 모델 4개와 예시 프리팹 1개 생성.

StreetLamp: 단일 암 가로등, 높이 약 4.27유닛. 조명 헤드는 로컬 +Z 방향.
TrafficSignal: 세로형 차량 신호등, 높이 약 3.84유닛. 신호 렌즈는 +Z를 향합니다.
BusShelter: 지붕/벤치/후면 패널/시간표 모형을 포함한 정류장. 폭 4.35, 깊이 약 2.0, 높이 2.67유닛.
BusStopSign: 독립형 버스 표지판, 높이 2.76유닛.
CityFacilitiesExample: 네 모델을 모아둔 확인용 예시. 바닥/도로/인도는 포함하지 않습니다.

배치
+Y 위, 루트 Y=0이 기초 바닥. 기존 도시 인도 위에는 Y=.202에 배치합니다.
가로등은 +Z가 차도를 향하도록 회전하세요. 신호등은 접근하는 차량을 향하게 배치합니다.
정류장 +Z가 출입구입니다. 깊이가 인도 폭과 비슷하므로 보행 통로 확보가 필요합니다.
정류장 뒤쪽 패널은 불투명 재질입니다. 실제 투명 유리가 아닙니다.
시간표의 선과 버스 아이콘은 장식 메시로 실제 노선 정보를 의미하지 않습니다.

발광과 동작
LampLens와 SignalLensRed에는 발광 재질 적용. 실제 Light 컴포넌트는 포함하지 않습니다.
노면을 비추려면 가로등 헤드 아래 (0,4.08,1.12)에 Light를 추가하고 아래쪽을 향하게 설정하세요.
Bloom은 프로젝트 후처리에서 별도 설정해야 합니다.
SignalLensRed / SignalLensAmber / SignalLensGreen은 개별 메시이며 기본 상태는 빨간색 활성 표현입니다.
신호 주기/차량 정지/보행 신호/조명 스위치 코드는 포함하지 않습니다.
각 프리팹의 신호 상태를 개별 변경하려면 공용 재질을 직접 바꾸기보다 MaterialPropertyBlock 또는 별도 재질 인스턴스를 사용하세요.

Built-in/URP 재질 지원, HDRP 별도 설정.
기둥/기초/정류장 주요 구조물에 정적 MeshCollider 적용, 작은 장식 충돌은 생략.
OBJ 4개와 MTL, Unity 생성기/메시 데이터, Python 소스, 미리보기 포함.
Python 재생성에 numpy/Pillow 필요. Unity 사용에는 Python 불필요.
프리뷰는 모델별 표시 배율이 다릅니다.
메시 인덱스/퇴화 면/유한 좌표 검사와 미리보기 확인 완료. Unity 내부 컴파일/실행 미검증.

다음 제작: 가로수·벤치·쓰레기통·볼라드.
