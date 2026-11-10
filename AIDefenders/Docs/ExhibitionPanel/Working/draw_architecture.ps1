Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap 6000,1800
$bmp.SetResolution(300,300)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = 'AntiAlias'
$g.TextRenderingHint = 'AntiAliasGridFit'
$g.Clear([System.Drawing.Color]::White)
$ink = [System.Drawing.Brushes]::Black
$blue = [System.Drawing.ColorTranslator]::FromHtml('#23779A')
$pen = New-Object System.Drawing.Pen $blue,8
$arrow = New-Object System.Drawing.Pen $blue,12
$arrow.CustomEndCap = New-Object System.Drawing.Drawing2D.AdjustableArrowCap 10,12
$title = New-Object System.Drawing.Font '맑은 고딕',82,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$head = New-Object System.Drawing.Font '맑은 고딕',66,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$body = New-Object System.Drawing.Font '맑은 고딕',53,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$fmt = New-Object System.Drawing.StringFormat
$fmt.Alignment = 'Center'; $fmt.LineAlignment = 'Center'
function Label($text,$x,$y,$w,$h,$font) { $g.DrawString($text,$font,$ink,[System.Drawing.RectangleF]::new($x,$y,$w,$h),$fmt) }
function Node($x,$y,$heading,$description) {
    $rect=[System.Drawing.Rectangle]::new($x,$y,1160,310)
    $fill=New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#ECF5F9'))
    $g.FillRectangle($fill,$rect); $g.DrawRectangle($pen,$rect)
    Label $heading $x ($y+25) 1160 95 $head
    Label $description ($x+30) ($y+130) 1100 145 $body
    $fill.Dispose()
}
Label 'AI 디펜더스 시스템 아키텍처' 180 50 5640 130 $title
Label '01   데이터 제작 및 모델 학습' 180 240 5640 90 $head
$xs=@(180,1670,3160,4650)
Node $xs[0] 370 '데이터셋 팩토리' "학습용 장면 구성`n이미지 자동 생성"
Node $xs[1] 370 '학습 데이터' "객체 이미지`nYOLO 형식 라벨"
Node $xs[2] 370 'YOLO · PyTorch' "객체 탐지 모델 학습`n아군·적 클래스 학습"
Node $xs[3] 370 '학습 모델' "가중치 파일 (.pt)`n추론 서버에서 로드"
for($i=0;$i -lt 3;$i++) { $g.DrawLine($arrow,($xs[$i]+1185),525,($xs[$i+1]-35),525) }
Label '02   실시간 게임 연동' 180 840 5640 90 $head
Node $xs[0] 980 'Unity CCTV' "게임 화면 캡처`nJPG 이미지 전송"
Node $xs[1] 980 'Python · FastAPI' "YOLO 객체 탐지`n클래스·신뢰도·좌표 반환"
Node $xs[2] 980 'Unity 피아식별' "탐지 결과와 대상 연결`n아군·적 구분 및 표적 지정"
Node $xs[3] 980 '포탑 · 전투 평가' "자동 조준 및 공격`n전투 결과·식별 정확도 표시"
for($i=0;$i -lt 3;$i++) { $g.DrawLine($arrow,($xs[$i]+1185),1135,($xs[$i+1]-35),1135) }
$g.DrawLine($pen,5230,705,5230,770)
$g.DrawLine($pen,5230,770,2250,770)
$g.DrawLine($arrow,2250,770,2250,950)
Label '학습 가중치 적용' 2990 704 900 65 $body
$g.DrawRectangle($pen,180,1460,5630,220)
Label '성장 및 저장   |   전투 결과 → 연구 포인트 → 포탑 영구 연구   /   진행 정보는 로컬 JSON으로 저장' 220 1490 5550 160 $body
$target=Join-Path $PSScriptRoot '../Submission/Originals/02_SystemArchitecture.png'
$bmp.Save([System.IO.Path]::GetFullPath($target),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$pen.Dispose();$arrow.Dispose();$title.Dispose();$head.Dispose();$body.Dispose();$fmt.Dispose()
Write-Output 'Architecture image exported: 6000 x 1800'
