AI Defenders 사막 소품 세트 - 제작 순서 04

설치
Assets/AIDefendersDesertProps 폴더를 Unity 프로젝트 Assets에 복사합니다.
Tools > AI Defenders > Create Desert Props Kit 실행.
Assets/AIDefendersDesertPropsGenerated에 모델 7개와 예시 프리팹 1개 생성.

PalmTree: 야자수 1종, 높이 약 3.63유닛. 잎은 양면 메시, 바람 애니메이션 없음.
Sandbag: 모래주머니 낱개, 약 .88x.46, 높이 .31유닛.
SandbagWall: 모래주머니 11개를 쌓은 벽, 폭 3.34, 높이 .86유닛.
DrumBlue / DrumRust: 드럼통 색상 2종, 높이 약 .99유닛.
SupplyCrate: 보강띠와 손잡이가 있는 보급 상자, 높이 약 .98유닛.
FieldTent: 천막. 줄/말뚝 포함 폭 약 5.15, 길이 4.49, 높이 2.49유닛.
DesertPropsExample: 천막과 소품을 모은 예시 배치. 바닥/도로/건물은 포함하지 않습니다.

배치
+Y 위, +Z 정면. 각 프리팹 루트 Y=0을 사막 바닥 높이에 맞춰 배치하세요.
언덕 위에 배치할 때는 지표면 높이에 맞춰 Y를 조절하세요.
나무와 천막이 적/아군 이동 경로를 가리지 않도록 도로 바깥에 배치하세요.
천막은 배경용 외관이며 어두운 입구는 장식입니다. 출입 가능한 실내/개폐 애니메이션 없음.
상자 열기, 드럼통 폭발, 엄폐 판정, 나무 흔들림 등의 게임 코드는 포함하지 않습니다.
기본 단색 재질의 로우폴리 메시이며 실사 텍스처는 포함하지 않습니다.

Built-in/URP 재질 지원. HDRP는 별도 재질 설정.
정적 MeshCollider 포함. 야자수 잎과 천막 줄에는 콜라이더를 붙이지 않습니다.
낱개 소품을 물리 오브젝트로 움직이려면 용도에 맞는 단순/Convex Collider와 Rigidbody로 설정하세요.
OBJ 7개, 공용 MTL, Unity 생성기/데이터, Python 생성 소스, 미리보기 포함.
Python 재생성에 numpy/Pillow 필요. Unity 사용에는 Python 불필요.
메시 인덱스/퇴화 면/유한 좌표 검사 및 미리보기 확인 완료.
Unity 내부 컴파일/실행은 미검증입니다.
프리뷰는 모델마다 표시 배율이 다릅니다.

다음 제작: 도시 인도·연석·화단.
