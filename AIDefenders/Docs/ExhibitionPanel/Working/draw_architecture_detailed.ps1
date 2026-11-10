Add-Type -AssemblyName System.Drawing
$bmp=New-Object System.Drawing.Bitmap 4500,3000
$bmp.SetResolution(300,300)
$g=[System.Drawing.Graphics]::FromImage($bmp)
$g.ScaleTransform(1.25,1.25)
$g.SmoothingMode='AntiAlias';$g.TextRenderingHint='AntiAliasGridFit'
$g.Clear([System.Drawing.Color]::White)
$ink=New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#17334A'))
$title=New-Object System.Drawing.Font '맑은 고딕',94,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$head=New-Object System.Drawing.Font '맑은 고딕',57,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$body=New-Object System.Drawing.Font '맑은 고딕',44,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$small=New-Object System.Drawing.Font '맑은 고딕',37,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$fmt=New-Object System.Drawing.StringFormat
$fmt.Alignment='Center';$fmt.LineAlignment='Center'
function Text($s,$x,$y,$w,$h,$font){$g.DrawString($s,$font,$ink,[System.Drawing.RectangleF]::new($x,$y,$w,$h),$fmt)}
function Node($x,$y,$w,$h,$heading,$desc,$tech,$color){
 $c=[System.Drawing.ColorTranslator]::FromHtml($color)
 $pen=New-Object System.Drawing.Pen $c,5
 $brush=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(20,$c))
 $g.FillRectangle($brush,$x,$y,$w,$h);$g.DrawRectangle($pen,$x,$y,$w,$h)
 Text $heading ($x+15) ($y+20) ($w-30) 95 $head
 Text $desc ($x+25) ($y+125) ($w-50) ($h-200) $body
 Text $tech ($x+15) ($y+$h-70) ($w-30) 55 $small
 $pen.Dispose();$brush.Dispose()
}
$arrow=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#527B96')),10
$arrow.CustomEndCap=New-Object System.Drawing.Drawing2D.AdjustableArrowCap 7,8
$line=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#527B96')),10
function Link($x1,$y1,$x2,$y2){$g.DrawLine($arrow,$x1,$y1,$x2,$y2)}
Text 'AI 디펜더스 시스템 구성도' 100 55 3400 130 $title
Text '사진으로 AI를 학습시키고, 게임 속 인식 결과를 포탑의 행동으로 연결합니다.' 100 205 3400 85 $body
Text '1. AI 준비하기' 100 325 3400 70 $head
Node 180 440 840 370 '데이터셋 제작' "양·늑대가 있는 장면 촬영`n사진과 정답 표시 만들기" '데이터셋 팩토리 · 이미지 / 라벨' '#258AAF'
Node 1380 440 840 370 'AI 모델 학습' "사진과 정답을 함께 학습`n양·늑대의 특징 익히기" 'YOLO · PyTorch' '#7861AD'
Node 2580 440 840 370 '학습 모델 준비' "학습한 내용을 파일로 저장`n인식 서버에서 불러오기" '가중치 파일 (.pt)' '#7861AD'
Link 1055 625 1335 625;Link 2255 625 2535 625
$g.DrawLine($line,3000,850,3000,930);$g.DrawLine($line,3000,930,1350,930);Link 1350 930 1350 1090
Text '학습한 AI를 적용' 1850 840 900 70 $small
Text '2. 게임 중에 판단하고 행동하기' 100 985 3400 75 $head
Node 100 1140 700 410 '게임 속 카메라' "실제 전투 장면 촬영`n사진을 서버로 보내기" 'Unity · CCTV · JPG 전송' '#258AAF'
Node 1000 1140 700 410 'AI 인식 서버' "양·늑대의 종류와 위치 찾기`n인식 결과를 게임으로 전달" 'Python · FastAPI · YOLO' '#7861AD'
Node 1900 1140 700 410 '아군·적 구별' "인식 결과를 실제 대상과 연결`n공격할 적을 포탑에 알리기" 'Unity · 피아식별 / 표적 지정' '#319984'
Node 2800 1140 700 410 '포탑 자동 공격' "적을 향해 조준하고 발사`n기지를 지키는 전투 진행" 'Unity · 포탑 / 전투 시스템' '#C18A35'
Link 830 1345 955 1345;Link 1730 1345 1855 1345;Link 2630 1345 2755 1345
$g.DrawLine($line,3150,1590,3150,1680);$g.DrawLine($line,3150,1680,600,1680);Link 600 1680 600 1840
Text '전투 기록과 식별 결과' 1350 1580 1100 70 $small
Text '3. 결과를 확인하고 포탑 성장시키기' 100 1740 3400 75 $head
Node 180 1890 840 330 '전투 결과 확인' "점수·처치 수·식별 정확도 확인`n전투 결과에 따른 연구 포인트" '결과 화면' '#C18A35'
Node 1380 1890 840 330 '포탑 영구 연구' "연구 포인트로 성능 강화`n다음 전투에도 강화 효과 적용" '포탑 연구소' '#319984'
Node 2580 1890 840 330 '진행 정보 저장' "보유 연구 포인트·연구 상태 저장`n게임을 다시 켜도 이어서 이용" '내 컴퓨터에 저장 · JSON' '#527B96'
Link 1055 2055 1335 2055;Link 2255 2055 2535 2055
Text '전체 흐름   데이터 제작 → AI 학습 → 실시간 인식 → 자동 방어 → 결과 확인 → 영구 연구' 100 2270 3400 80 $body
$target=Join-Path $PSScriptRoot '../Submission/Originals/02_SystemArchitecture_Detailed.png'
$bmp.Save([IO.Path]::GetFullPath($target),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$ink.Dispose();$title.Dispose();$head.Dispose();$body.Dispose();$small.Dispose();$fmt.Dispose();$arrow.Dispose();$line.Dispose()
