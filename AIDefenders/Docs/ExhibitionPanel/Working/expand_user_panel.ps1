$ErrorActionPreference='Stop'
$app=New-Object -ComObject PowerPoint.Application
$deck=$null
$out=Join-Path $PSScriptRoot '../Revised'
New-Item -ItemType Directory -Path $out -Force | Out-Null
$out=[IO.Path]::GetFullPath($out)
$name='AI 디펜더스_소프트웨어학과_동현우, 유기현_문구보강'
function SetBody($id,$text,$size,$height){
 $shape=$null
 foreach($s in $slide.Shapes){if($s.Id -eq $id){$shape=$s;break}}
 if(!$shape){throw "Shape not found: $id"}
 $shape.TextFrame.AutoSize=0
 $shape.TextFrame.TextRange.Text=$text.Replace("`n","`r")
 $shape.Height=$height
 $shape.TextFrame.WordWrap=-1
 $shape.TextFrame.TextRange.Font.Name='맑은 고딕'
 $shape.TextFrame.TextRange.Font.NameFarEast='맑은 고딕'
 $shape.TextFrame.TextRange.Font.Size=$size
 $shape.TextFrame.TextRange.Font.Color.RGB=0
 $shape.TextFrame.TextRange.ParagraphFormat.SpaceAfter=0
 $shape.TextFrame.TextRange.ParagraphFormat.SpaceWithin=1.12
 $shape.TextFrame.MarginTop=0;$shape.TextFrame.MarginBottom=0
 if($shape.TextFrame.TextRange.BoundHeight -gt $shape.Height+1){throw "Text overflow: $id"}
}
$overview=@'
AI 디펜더스는 직접 만든 인식 모델로
아군과 적을 구분하며 기지를 지키는
Unity 기반 3D 타워 디펜스 게임이다.

플레이어는 데이터셋 제작과 모델 학습,
포탑 배치·업그레이드, 영구 연구를
반복하며 자신만의 방어 전략을 구성한다.

학습한 모델의 판단이 포탑의 공격으로
이어지며, 식별 결과를 전투에서 확인한다.
AI의 학습과 활용 과정을 게임으로 경험한다.
'@
$technology=@'
개발 환경  Unity · C# · Python
AI 연동  YOLO · PyTorch · FastAPI

구현 흐름
① 이미지·정답 표시 생성 → 모델 학습
② CCTV 영상 전송 → 객체 탐지·피아식별
③ 표적 지정·자동 공격 → 전투 평가·연구
게임 콘텐츠
초원 낮·저녁·밤 / 사막 낮 / 도시 스테이지
정밀 사격·기관총·미사일·레이저 포탑
전투 중 단계 강화와 연구소의 영구 연구
연구 포인트·연구 상태를 로컬 파일로 저장
'@
$conclusion=@'
데이터 제작·모델 학습·실시간 인식·포탑 제어를 연결한 게임을 구현하였다.
플레이어가 만든 모델의 피아식별 결과를 점수와 식별 정확도로 확인할 수 있다.
포탑 선택과 배치, 전투 중 강화, 영구 연구를 통해 반복 플레이의 성장 요소를 구성하였다.
이를 통해 AI의 판단을 관찰하고 방어 전략을 바꾸는 플레이 경험을 제공한다.
향후 다양한 학습 데이터를 확보해 인식의 안정성을 높이고, 챕터별 난이도와
포탑 밸런스를 조정하여 전략의 다양성과 플레이 완성도를 개선할 계획이다.
'@
try{
 $deck=$app.Presentations.Open('C:\Users\user\Downloads\AI 디펜더스_소프트웨어학과_동현우, 유기현.pptx',0,0,0)
 $slide=$deck.Slides.Item(1)
 SetBody 20 $overview 30 450
 SetBody 29 $technology 28 450
 SetBody 38 $conclusion 30 280
 $deck.SaveAs((Join-Path $out ($name+'.pptx')),24,-1)
 $deck.SaveAs((Join-Path $out ($name+'.pdf')),32)
 $slide.Export((Join-Path $out '문구보강_미리보기.png'),'PNG',1800,2200)
 @("[캡스톤 과제 개요]",$overview,'',"[핵심 구현 기술]",$technology,'',"[결론]",$conclusion) | Set-Content (Join-Path $out '추가문구.txt') -Encoding UTF8
 Write-Output 'Revised PPTX, PDF, preview and text exported. No text overflow.'
}finally{if($deck){$deck.Close()};$app.Quit()}
