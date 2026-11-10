Add-Type -AssemblyName System.Drawing
$bmp=New-Object System.Drawing.Bitmap 3600,2400
$bmp.SetResolution(300,300)
$g=[System.Drawing.Graphics]::FromImage($bmp)

$g.SmoothingMode='AntiAlias';$g.TextRenderingHint='AntiAliasGridFit'
$g.Clear([System.Drawing.Color]::White)
$ink=New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#17334A'))
$title=New-Object System.Drawing.Font '맑은 고딕',94,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$head=New-Object System.Drawing.Font '맑은 고딕',64,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$body=New-Object System.Drawing.Font '맑은 고딕',50,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$small=New-Object System.Drawing.Font '맑은 고딕',40,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
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
Text 'AI 디펜더스 시스템 구성도' 100 80 3400 130 $title

Text '01   데이터 제작과 AI 학습' 100 385 3400 80 $head
Node 400 520 1200 400 '데이터셋 제작' "게임 속 양·늑대 사진을 만들고`n각 대상의 이름과 위치를 표시" '데이터셋 팩토리 · 사진 + 정답 표시' '#258AAF'
Node 2000 520 1200 400 'AI 학습' "사진과 정답으로 양·늑대의 특징 학습`n학습한 모델을 인식 서버에 적용" 'YOLO · PyTorch · 학습 모델 (.pt)' '#7861AD'
Link 1650 720 1930 720
$g.DrawLine($line,2600,955,2600,1040)
$g.DrawLine($line,2600,1040,1800,1040)
Link 1800 1040 1800 1230
Text '학습한 모델 사용' 2050 955 550 65 $small
Text '02   실시간 인식과 기지 방어' 100 1100 1500 80 $head
Node 140 1280 1000 440 '게임 카메라' "전투 장면을 촬영해`n인식 서버로 사진 전송" 'Unity · CCTV' '#258AAF'
Node 1300 1280 1000 440 'AI 인식 · 피아식별' "사진 속 대상의 종류와 위치 확인`n실제 대상과 연결해 아군·적 구별" 'Python · FastAPI · YOLO → Unity' '#319984'
Node 2460 1280 1000 440 '포탑 자동 공격' "구별한 적을 표적으로 지정`n포탑이 조준하고 공격해 기지 방어" 'Unity · 포탑 시스템' '#C18A35'
Link 1170 1500 1255 1500
Link 2330 1500 2415 1500
Link 2960 1765 2960 1940
Text '전투 기록' 3020 1780 370 70 $small
Text '03   결과 확인과 성장' 100 1810 2600 80 $head
Node 400 1990 2800 300 '전투 결과 · 영구 연구' "점수·식별 정확도 확인 → 연구 포인트 획득 → 포탑 성능 강화" '연구 효과는 다음 전투에도 적용 · 진행 정보는 내 컴퓨터에 저장' '#527B96'
$target=Join-Path $PSScriptRoot '../Submission/Originals/02_SystemArchitecture_Balanced_3x2.png'
$bmp.Save([IO.Path]::GetFullPath($target),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$ink.Dispose();$title.Dispose();$head.Dispose();$body.Dispose();$small.Dispose();$fmt.Dispose();$arrow.Dispose();$line.Dispose()
