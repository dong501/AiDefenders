AI Defenders - 도시 놀이터 소품 세트

4종: PlaygroundSlide(미끄럼틀), PlaygroundSwing(2인 그네),
PlaygroundSeesaw(시소), PlaygroundSandbox(모래놀이터).
기존 도시/공원 소품에 어울리는 청록/붉은색/목재색의 단순한 형태.
1,148 삼각형. 바닥 Y=0, 1단위=1m. 정확한 크기는 ModelInfo.json 참고.
미끄럼틀 높이 약2.11m/길이4.92m. 그네 높이 약2.50m.
모래놀이터 크기4m x 3m, 작은 고정 양동이와 삽 포함.

Unity 적용:
1. 압축 해제 후 Assets/AIDefendersPlayground를 프로젝트 Assets로 복사.
2. Tools > AI Defenders > Create Playground 실행.
3. Assets/AIDefendersPlaygroundGenerated 아래 개별 프리팹 또는 배치 예제 사용.
기존 출력 폴더가 있으면 새 경로 생성. Built-in Standard / URP Lit 재질 지원.
HDRP는 별도 설정 필요. OBJ+MTL, 생성 소스, 크기 정보, PNG 미리보기 포함.
FBX/unitypackage는 포함하지 않는다. 외부 텍스처 없이 색상 재질 사용.

모두 정적 배경 모델이다.
그네 흔들림/시소 움직임/캐릭터 탑승/미끄러지기/모래 변형은 포함하지 않는다.
사슬은 가는 직선 메시이며 물리 연결 관절이 없다.
부품별 정적 MeshCollider 적용. 사슬과 작은 장난감 장식에는 충돌체 없음.
Rigidbody, Animator, AnimationClip, NavMesh는 포함하지 않는다.
이동/등반/체험형 상호작용을 사용하려면 프로젝트에서 별도로 구현해야 한다.
충돌체를 모델 전체의 큰 루트 박스로 만들지 않아 구조물 사이 공간을 보존한다.

Preview.png는 실제 메시의 소프트웨어 렌더이며 Unity 캡처가 아니다.
카탈로그는 각 소품을 개별 확대했으므로 상대적인 크기를 비교할 수 없다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
