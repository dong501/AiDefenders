AI Defenders — 나무 울타리·목장 출입문 세트

구성: 정적 울타리 6종 + 작동 출입문 2종, 총 8종 / 888 삼각형
FenceStraight: 기둥 중심 간격 4
FenceHalf: 기둥 중심 간격 2
FenceBraced: 대각 보강 울타리, 기둥 간격 4
FenceDamaged: 가운데가 끊어진 파손형, 기둥 간격 4
FenceCorner: 교차점(0,0)에서 W로 4, N으로 4 이어지는 L자
FencePost: 독립 기둥
GateSingle: 단문, 기둥 간격 4
GateDouble: 양문, 기둥 간격 4
단위는 Unity 1단위=1m 기준입니다. 기둥 캡 포함 높이 1.48입니다.

Unity 적용
Assets/AIDefendersMeadowFence 폴더를 프로젝트 Assets에 복사합니다.
컴파일 후 Tools > AI Defenders > Create Meadow Fence 메뉴를 실행합니다.
Assets/AIDefendersMeadowFenceGenerated 폴더에 개별 프리팹과 재질·메시,
MeadowFenceSample.prefab이 생성됩니다. 반복 실행 시 새 폴더에 생성합니다.
Built-in과 URP를 지원합니다. HDRP는 별도 재질 설정이 필요합니다.

배치
모든 모델 바닥은 Y=0입니다. 직선과 문은 X축 방향, 기둥은 X=-2/+2입니다.
4단위 위치 간격, Y축 90도 회전으로 기존 초원 타일 주변에 연결하세요.
반칸 모델은 X=-1/+1이며 코너 모델은 원점이 꺾이는 기둥입니다.
기둥과 캡 때문에 실제 외곽 폭은 중심 간격보다 조금 큽니다.
기둥이 같은 위치에 겹치는 연결부는 한쪽 모듈의 해당 PostL/PostR와
그에 대응하는 _Cap 자식을 함께 비활성화하세요. 샘플은 중복 기둥을 제거합니다.
미리보기의 녹색 바닥은 확인용으로 샘플 프리팹에는 포함되지 않습니다.

출입문 동작
문 루트의 MeadowGate 컴포넌트가 GateLeft / GateRight 경첩을 회전시킵니다.
namespace: AIDefenders.Assets.MeadowFence
Open(): 열기 / Close(): 닫기 / Toggle(): 전환 / SetOpen(bool): 목표 상태 지정
기본 열림 각도 95도, 초당 90도입니다. 단문은 약 1.06초에 열립니다.
양문도 양쪽 날개가 동시에 움직이므로 시간은 같습니다.
startOpen으로 시작 상태, autoDemo로 자동 개폐 시연을 설정할 수 있습니다.
기본은 닫힘이며 자동 시연은 꺼져 있습니다. autoDemo를 끄면 직접 호출로 제어합니다.
AnimationClip/Animator 방식이 아닌 C# 절차적 동작입니다.
플레이 중 동작하며 소스 OBJ는 문이 닫힌 정적 모델입니다.
루트는 고정해 두고 사용하세요. 움직이는 차량에 장착하는 용도는 아닙니다.

충돌
나무 부품마다 근사 BoxCollider를 생성합니다. 대각 보강재는 메시의 로컬
축 정렬 경계 상자이므로 실제 보강재보다 넓은 영역을 막을 수 있습니다.
문짝 경첩에 kinematic Rigidbody가 있고 MoveRotation으로 문짝의 복합 충돌체를 움직입니다.
문이 열려도 NavMesh 경로 갱신이나 적 이동 제어가 자동으로 되지는 않습니다.
프로젝트 이동 방식에 맞게 문 상태와 경로를 연동하세요.
금속 장식과 빗장은 시각 요소이며 별도 잠금/빗장 동작은 없습니다.
파손형은 고정된 형태이며 실시간 파괴 시스템이 아닙니다.

파일
Models: OBJ 8종과 공용 MTL
Assets: Unity 에디터 생성기, 메시 JSON, MeadowGate 런타임 스크립트
Preview.png: 크기를 각각 맞춘 카탈로그
SampleLayout.png: 연결 배치 예시
GateMotion.gif: 실제 모델의 양문 개폐 미리보기
ModelInfo.json: 크기와 삼각형 수
Python 소스: build_model.py, render_preview.py, render_layout.py
Python 3 / numpy / Pillow로 소스 재생성이 가능합니다.
텍스처 없이 색상 재질을 사용하며 텍스처용 UV는 없습니다.

검증
8개 모델의 유한 좌표, 삼각형 면적·인덱스, 바깥 방향 법선, Y=0 바닥,
문 회전 시 경첩 위치 및 높이 보존을 확인했습니다.
미리보기는 실제 메시의 소프트웨어 렌더이며 Unity 캡처가 아닙니다.
Unity 환경이 없어 실제 컴파일·플레이 및 물리 충돌 검증은 하지 못했습니다.
