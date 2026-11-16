AI Defenders - 사막 시장 소품 세트

모델 6종:
MarketCanopy: 줄무늬 천막과 고정된 판매 카운터
FruitStand: 3색 과일이 담긴 목제 진열대
PotteryDisplay: 입구가 열린 도자기 5개와 바닥 매트
GrainSacks: 묶인 곡물 자루 4개 묶음
CarpetRack: 카펫 2장이 걸린 진열대
Handcart: 빈 짐칸과 고정 바퀴가 있는 목제 손수레

Unity 적용:
1. 압축 해제 후 Assets/AIDefendersDesertMarket 폴더를 프로젝트 Assets 안에 복사.
2. Tools > AI Defenders > Create Desert Market 실행.
3. Assets/AIDefendersDesertMarketGenerated에서 개별 프리팹 또는 배치 예제 사용.
같은 이름 출력 폴더가 있으면 새 경로 생성. Built-in Standard / URP Lit 재질 지원.
HDRP 재질은 별도 설정 필요.
Models 폴더에는 OBJ + MTL 포함. FBX/unitypackage는 포함하지 않는다.
생성 소스, 모델 크기 정보(ModelInfo.json), 실제 메시 미리보기 포함.
외부 텍스처 없이 색상 재질 사용. 총 5,460 삼각형.

1단위=1m 기준. 모든 소품 바닥 Y=0. 정면 +Z.
카탈로그에서는 소품별 확대 크기가 다르므로 실제 크기는 ModelInfo.json 참고.
천막은 너비3.24m/높이2.75m, 카운터와 천막 기둥이 하나의 모델에 포함된다.
각종 상품, 자루, 카펫은 프리팹의 고정 구성물이며 개별 상호작용은 없다.
모든 소품 정적. 바퀴 회전/이동, 천 흔들림, 상점 거래, 수확/파괴 시스템은 없다.
인간 상인, 조명, 소리, Rigidbody, NavMesh는 포함하지 않는다.
구조 부품별 정적 MeshCollider 사용. 과일과 천막 천/작은 카펫 장식에는 충돌체 없음.
손수레 짐칸, 도자기 입구를 루트 박스 충돌체로 막지 않는다.
복잡한 물리 상호작용을 위한 충돌체 설정은 프로젝트에 맞춰 조정해야 한다.
Preview.png는 실제 메시의 소프트웨어 렌더이며 Unity 캡처가 아니다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
