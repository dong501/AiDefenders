AI Defenders - 소형 무인 정찰 로봇

4륜 차체, 광학 카메라 헤드, 안테나, 배터리 덮개, 전방 센서가 있는 정찰용 로봇.
무장은 없는 일반화된 스타일 모델이다.
2,036 삼각형. 폭약1.42m/높이1.27m/길이2.03m.
정면 +Z, 바퀴 접지 Y=0. 1단위=1m. 색상 재질, 외부 텍스처 없음.

Unity 적용:
1. 압축 해제 후 Assets/AIDefendersScoutRover를 프로젝트 Assets로 복사.
2. Tools > AI Defenders > Create Scout Rover 실행.
3. Assets/AIDefendersScoutRoverGenerated 아래 ScoutRover.prefab 또는 ScoutRoverDriveDemo 사용.
기존 출력 폴더가 있으면 새 경로 생성. Built-in Standard / URP Lit 지원.
HDRP는 별도 설정 필요. OBJ+MTL, 생성 소스, 미리보기 PNG/GIF 포함.
FBX/unitypackage는 포함하지 않는다. OBJ는 정지 상태 메시다.

이동:
ScoutRoverDrive.mode = Parked(기본), DemoStraight, External.
DemoStraight는 1m/s로 2m 전진 후 후진 반복.
External은 별도 코드로 루트를 이동하면 바퀴 회전과 앞바퀴 조향을 표시.
바퀴 반경0.26m, 축간거리1.12m. Stop(), Resume(), RestartDemo() 제공.
평면 이동과 균일한 양수 스케일 기준. 실제 차량 물리/경로 탐색/충돌 회피는 없다.

카메라 모션:
RoverScanner.SetScanning(bool), SetAngle(float) 제공.
기본 탐색 좌우50도, 주기4초. Inspector에서 범위와 주기 변경 가능.
기본 프리팹은 정차/탐색 중지, DriveDemo는 이동/탐색 동작.
카메라는 외형과 회전 모션이며 실제 Camera 컴포넌트, 영상 송신, 인식 기능은 없다.
이동 및 헤드 모션은 런타임 Transform 제어. Animator/AnimationClip 미포함.

차체 Chassis에 단순화된 BoxCollider 사용.
바퀴와 센서 충돌체, Rigidbody/WheelCollider/NavMesh는 포함하지 않는다.
센서 색상은 시각 요소이며 실제 조명/발광 효과는 없다.
아군/적군 태그, 분류 학습, 체력, 피격, 점수는 프로젝트에서 연결해야 한다.
Preview.png/DrivePreview.gif는 실제 메시 소프트웨어 렌더이며 Unity 캡처가 아니다.
실제 Unity 편집기 컴파일 및 플레이 검증은 수행하지 못했다.
