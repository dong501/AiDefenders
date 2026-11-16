AI Defenders - 사막 유적 소품 세트

모델 6종:
RuinGate: 석조 관문 (중앙 통로 개방)
BrokenColumn: 상단이 부서진 기둥과 작은 잔해
FallenColumn: 쓰러진 기둥과 떨어진 주두
BrokenRuinWall: 높이가 불규칙한 파손 석벽
RuinSteps: 장식용 석조 계단과 상부 단
RuinObelisk: 청록색 장식이 있는 석주

Unity 적용:
1. 압축 해제 후 Assets/AIDefendersDesertRuins 폴더를 프로젝트 Assets로 복사.
2. Tools > AI Defenders > Create Desert Ruins 실행.
3. Assets/AIDefendersDesertRuinsGenerated 안의 개별 프리팹 또는 배치 예제 사용.
같은 이름의 출력 폴더가 있으면 새 경로를 만든다.
Built-in Standard / URP Lit 재질 생성. HDRP는 별도 설정 필요.
외부 텍스처 없이 단색 재질 사용. 범용 OBJ + MTL 및 생성 소스 포함.
FBX, unitypackage 파일은 포함하지 않는다.

1단위=1m 기준. 바닥 Y=0. 크기는 ModelInfo.json 참고.
정적 구조 부품마다 MeshCollider 사용. 관문 개구부를 큰 박스가 막지 않는다.
장식에는 충돌체 없음. Rigidbody, 파괴 물리, 애니메이션, NavMesh는 포함하지 않는다.
기둥과 벽의 잔해는 이미 파손된 형태이며 실시간 붕괴하지 않는다.
계단은 정적 구조물이므로 이동 방식에 따른 경로/충돌 설정은 프로젝트에서 해야 한다.
6종 합계 1,456 삼각형.
Preview.png는 실제 메시의 소프트웨어 렌더링이며 각 모델을 개별 확대했다.
Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
