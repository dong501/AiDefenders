AI Defenders - 초원 농장 작물 및 돌우물 세트

6종: TilledField(빈 밭), WheatField(밀), CornField(옥수수),
VegetableField(양배추형 채소), PumpkinField(호박), StoneWell(돌우물).
5종 밭은 4m x 4m 타일. 작물은 정적인 장식이며 성장/수확/바람 애니메이션 없음.
돌우물은 빈 중앙, 고정된 축/크랭크/밧줄/양동이와 불투명 물 표면 포함.
물 흐름/두레박 조작/농작업 시스템은 포함하지 않는다.

Unity 적용:
1. 압축 해제 후 Assets/AIDefendersFarmCrops를 프로젝트 Assets 안으로 복사.
2. Tools > AI Defenders > Create Farm Crops 실행.
3. Assets/AIDefendersFarmCropsGenerated에서 개별 프리팹 또는 배치 예제 사용.
기존 출력 폴더가 있으면 새 경로를 생성한다.
Built-in Standard / URP Lit 재질 지원. HDRP는 별도 설정 필요.
Models 폴더 OBJ + MTL 및 모델/미리보기 생성 소스 포함. FBX는 포함하지 않는다.
외부 텍스처 없이 색상 재질 사용. 총 7,784 삼각형.

밭 배치 기준:
밭 지면 상단 Y=0, 타일 하부 Y=-0.25. 기존 초원 타일과 같은 지면 기준이다.
밭 중심은 X/Z 방향 4m 간격으로 연결한다. 두둑은 위로 약0.095m 돌출.
돌우물은 바닥 Y=0. 밭/우물 크기는 ModelInfo.json 참고.
배치 예제는 밭 5개와 우물만 포함하며 트랙터/헛간/초원 타일은 별도 모델이다.
재질 및 충돌 여부별 메시 병합으로 작물 개체 수를 줄였다.
밭 지면, 두둑 및 우물 구조물에 정적 MeshCollider 사용.
작물/물/금속 장식에는 충돌체 없음. Rigidbody/NavMesh 미포함.
우물 중앙을 큰 박스 충돌체로 막지 않는다.

Preview.png는 실제 메시를 소프트웨어 렌더링한 그림이며 Unity 캡처가 아니다.
모델은 카탈로그 안에서 개별 확대되어 표시된다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
