AI Defenders 사막 건물 세트 - 제작 순서 03

Assets/AIDefendersDesertBuildings 폴더를 Unity 프로젝트 Assets에 복사합니다.
Tools > AI Defenders > Create Desert Buildings Kit 실행.
Assets/AIDefendersDesertBuildingsGenerated에 모델 4개와 예시 배치 1개 생성.

HouseSmall: 작은 단층 주택. 차양, 옥상 난간과 환기구 포함.
HouseTall: 2층형 주택. 차양 기둥, 층간 띠, 옥상 구조물과 후면 사다리 포함.
Warehouse: 셔터 창고. 박공지붕, 지붕 리브, 보조 출입문 포함.
Checkpoint: 초소, 차단봉, 지지대, 표지판과 볼라드 포함.
DesertBuildingsExample: 네 모델을 떨어뜨려 배치한 예시. 바닥/도로는 포함하지 않습니다.

배치
+Y 위, +Z 정면. 루트 Y=0을 기존 사막 바닥 Y=0에 맞춰 배치합니다.
건물은 10유닛 타일 전체를 채우는 규격이 아니며 타일 위에 자유 배치합니다.
Checkpoint 루트 X=0은 차단봉 중앙으로, 6유닛 도로 중심선과 맞춥니다.
차단봉 지지대 X=+-3.35, 초소는 도로 오른쪽 X=4.45에 있습니다.
검문소는 총 폭 약 10.75유닛이므로 인접 타일에 여유를 두세요.
차단봉이 도로를 가로막는 정적 모델입니다. 자유 통행 경로에는 차단봉을 숨기거나 별도 개폐 기능을 연결하세요.
BarrierArm과 BarrierStripe* 메시가 차단봉 구성 부품입니다.
모델은 배경용 외관이며 실내/출입 가능한 문/셔터 개폐/차단봉 애니메이션은 포함하지 않습니다.
사다리 역시 장식 모델입니다. 이동AI/충돌 회피/NavMesh는 별도 설정해야 합니다.

Built-in/URP 재질 지원, HDRP 별도 설정.
정적 MeshCollider 포함. 게임 성능에 맞춰 필요 시 단순 BoxCollider로 교체할 수 있습니다.
건물별 정확한 크기와 삼각형 수는 ModelInfo.json 참조.
OBJ 4개, MTL, Unity 생성기, 메시 데이터, Python 소스, 미리보기 포함.
프리뷰의 모델별 표시 배율은 다릅니다.
Python 재생성: numpy/Pillow 필요. Unity 사용: Python 불필요.
메시 인덱스/퇴화 삼각형 검사와 미리보기 확인 완료. Unity 내부 컴파일/실행 미검증.

다음 제작: 야자수, 모래주머니, 드럼통, 보급 상자, 천막.
