AI Defenders - 도시 생활시설 소품 세트

구성 6종:
VendingMachine - 음료 자판기
ATMTerminal - ATM 단말기
FireHydrant - 소화전
PostBox - 우체통
PhoneBooth - 전면이 열린 공중전화 부스
BicycleRack - 3칸 자전거 거치대 (자전거 미포함)

Unity 적용:
1. 압축 해제 후 Assets/AIDefendersStreetUtilities 폴더를 프로젝트 Assets 안으로 복사.
2. Tools > AI Defenders > Create Street Utilities 실행.
3. Assets/AIDefendersStreetUtilitiesGenerated 아래 개별 프리팹 또는 배치 예제 사용.
같은 이름의 폴더가 있으면 새 경로를 생성한다.
Built-in Standard / URP Lit 재질 생성. HDRP는 별도 재질 설정 필요.
Models 폴더: 범용 OBJ + MTL. FBX/unitypackage는 포함하지 않는다.
색상 재질 사용, 외부 텍스처 없음. 생성 소스 포함.

1단위=1m. 모든 모델 바닥 Y=0, 정면 +Z.
각 모델 크기는 ModelInfo.json 참고. 6종 총 2,504 삼각형.
모든 소품은 정적 배경 모델이며 기능/상호작용/애니메이션은 없다.
자판기의 상품, ATM 화면과 버튼, 소화전, 우체통, 전화기는 시각적 장식이다.
물 분사, 자동 판매, 금융 기능, 우편 처리, 통화 기능은 포함하지 않는다.
부스는 유리 없는 개방 프레임이며, 문/문 애니메이션이 없다.
구조 부품별 정적 MeshCollider 사용. Rigidbody/NavMesh는 포함하지 않는다.
부스 입구와 거치대 내부를 큰 박스 충돌체가 막지 않는다.
바닥 프레임 높이는 부스 0.11m, 거치대 발판 0.05m이다.

Preview.png는 실제 메시의 소프트웨어 렌더링이며 Unity 캡처가 아니다.
각 모델을 개별 확대했으므로 카탈로그의 크기는 서로 비교할 수 없다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
