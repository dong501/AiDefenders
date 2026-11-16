Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap 3600,2400
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
$title = New-Object System.Drawing.Font '맑은 고딕',88,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$head = New-Object System.Drawing.Font '맑은 고딕',60,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$body = New-Object System.Drawing.Font '맑은 고딕',44,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$fmt = New-Object System.Drawing.StringFormat
$fmt.Alignment = 'Center'; $fmt.LineAlignment = 'Center'
function Label($text,$x,$y,$w,$h,$font) { $g.DrawString($text,$font,$ink,[System.Drawing.RectangleF]::new($x,$y,$w,$h),$fmt) }
function Node($x,$y,$heading,$description) {
    $rect=[System.Drawing.Rectangle]::new($x,$y,720,410)
    $fill=New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#ECF5F9'))
    $g.FillRectangle($fill,$rect); $g.DrawRectangle($pen,$rect)
    Label $heading $x ($y+35) 720 95 $head
    Label $description ($x+25) ($y+145) 670 220 $body
    $fill.Dispose()
}
Label 'AI 디펜더스 시스템 아키텍처' 100 90 3400 150 $title
Label '01   데이터 제작 및 모델 학습' 100 370 3400 100 $head
$xs=@(120,1000,1880,2760)
Node $xs[0] 550 '데이터셋 팩토리' "학습용 장면 구성`n이미지 자동 생성"
Node $xs[1] 550 '학습 데이터' "객체 이미지`nYOLO 형식 라벨"
Node $xs[2] 550 'YOLO · PyTorch' "객체 탐지 모델 학습`n아군·적 클래스 학습"
Node $xs[3] 550 '학습 모델' "가중치 파일 (.pt)`n추론 서버에서 로드"
for($i=0;$i -lt 3;$i++) { $g.DrawLine($arrow,($xs[$i]+745),755,($xs[$i+1]-30),755) }
$g.DrawLine($pen,3120,990,3120,1120)
$g.DrawLine($pen,3120,1120,1360,1120)
$g.DrawLine($arrow,1360,1120,1360,1380)
Label '학습 가중치 적용' 1800 1020 1000 75 $body
Label '02   실시간 게임 연동' 100 1190 3400 100 $head
Node $xs[0] 1430 'Unity CCTV' "게임 화면 캡처`nJPG 이미지 전송"
Node $xs[1] 1430 'Python · FastAPI' "YOLO 객체 탐지`n클래스·신뢰도·좌표 반환"
Node $xs[2] 1430 'Unity 피아식별' "탐지 결과와 대상 연결`n아군·적 구분 및 표적 지정"
Node $xs[3] 1430 '포탑 · 전투 평가' "자동 조준 및 공격`n전투 결과·식별 정확도 표시"
for($i=0;$i -lt 3;$i++) { $g.DrawLine($arrow,($xs[$i]+745),1635,($xs[$i+1]-30),1635) }
$g.DrawRectangle($pen,120,2040,3360,230)
Label "성장 및 저장   |   전투 결과 → 연구 포인트 → 포탑 영구 연구`n진행 정보는 로컬 JSON으로 저장" 160 2060 3280 185 $body
$target=Join-Path $PSScriptRoot '../Submission/Originals/02_SystemArchitecture_3x2.png'
$bmp.Save([System.IO.Path]::GetFullPath($target),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$pen.Dispose();$arrow.Dispose();$title.Dispose();$head.Dispose();$body.Dispose();$fmt.Dispose()
Write-Output 'Architecture image exported: 3600 x 2400'
