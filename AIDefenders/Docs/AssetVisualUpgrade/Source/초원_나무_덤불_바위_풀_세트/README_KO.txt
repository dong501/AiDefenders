AI Defenders — 초원 자연물 세트

초원 타일과 함께 배치할 수 있는 로우폴리 정적 소품 12종입니다.
나무 3종: TreeOak, TreeRound, TreePine
덤불 2종: BushRound, BushWide
바위 3종: RockSmall, RockLarge, RockCluster
풀 2종: GrassTuft, GrassPatch
야생화: Wildflowers / 그루터기: TreeStump
전체 2,524 삼각형. 개별 크기와 삼각형 수는 ModelInfo.json을 참고하세요.

Unity 적용
1. Assets/AIDefendersGrasslandNature 폴더를 프로젝트의 Assets에 복사합니다.
2. 컴파일 후 Tools > AI Defenders > Create Grassland Nature를 실행합니다.
3. Assets/AIDefendersGrasslandNatureGenerated 아래 개별 프리팹 12종,
   메시·재질, GrasslandNatureSample.prefab이 생성됩니다.
반복 실행 시 기존 결과를 덮어쓰지 않고 새 폴더를 만듭니다.
Built-in 및 URP용 재질을 지원합니다. HDRP는 별도 재질 설정이 필요합니다.

배치 기준
모든 모델은 바닥 접점 Y=0, 위쪽 +Y입니다. 1단위=1m 기준 크기로 제작했습니다.
앞서 만든 4×4 초원 타일 위에 자유롭게 배치할 수 있습니다.
나무 높이는 약 2.9~3.5단위, 풀 높이는 약 0.3~0.4단위입니다.
타일에 맞추려면 땅의 높이에 모델 Y 위치를 맞추세요.
경사나 언덕에서는 지면에 살짝 묻히도록 조정하면 자연스럽습니다.

충돌과 성능
나무는 아래쪽 줄기 영역의 CapsuleCollider를 생성합니다. 수관에는 충돌이 없습니다.
바위·그루터기는 근사 BoxCollider를 생성합니다. 정밀 표면 충돌이 아닙니다.
덤불·풀·꽃은 장식용이며 충돌체가 없습니다.
NavMesh 장애물, 포탑 설치 제한, 흔들림·성장·벌목 애니메이션은 포함하지 않습니다.
많이 배치할 때는 프로젝트에 맞게 LOD/배칭/그림자 거리를 설정하세요.
재질은 색상 기반이며 텍스처와 텍스처용 UV는 없습니다.

포함 파일
Models: 외부 편집용 OBJ/MTL 12종
Assets: Unity 생성기 및 메시 데이터
Preview.png: 모델별 크기를 각각 맞춘 12종 카탈로그
SampleLayout.png: 같은 실제 크기 기준의 배치 예시
SampleLayout.json: 배치 위치 참고
build_model.py / render_preview.py / render_layout.py: 재생성 소스

샘플 프리팹은 소품만 포함합니다. 미리보기의 녹색 바닥은 크기 확인용이며,
실제 씬에는 기존 초원 지형 타일 또는 별도 지면을 배치하세요.
이미지는 원본 메시를 소프트웨어로 렌더링한 것으로 Unity 화면 캡처가 아닙니다.
유한 좌표, 삼각형 인덱스·면적, 바닥 접점, OBJ 내보내기를 확인했습니다.
Unity 에디터 환경이 없어 실제 컴파일 및 플레이 검증은 하지 못했습니다.

소스 재생성에는 Python 3, numpy, scipy, Pillow가 필요합니다.
build_model.py → render_preview.py → render_layout.py 순서로 실행하세요.
