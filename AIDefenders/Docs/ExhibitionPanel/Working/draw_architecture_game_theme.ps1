Add-Type -AssemblyName System.Drawing
$bmp=New-Object System.Drawing.Bitmap 3600,2400
$bmp.SetResolution(300,300)
$g=[System.Drawing.Graphics]::FromImage($bmp)

$g.SmoothingMode='AntiAlias';$g.TextRenderingHint='AntiAliasGridFit'
$g.Clear([System.Drawing.Color]::White)
$ink=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
$title=New-Object System.Drawing.Font '맑은 고딕',94,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$head=New-Object System.Drawing.Font '맑은 고딕',64,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$body=New-Object System.Drawing.Font '맑은 고딕',50,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$small=New-Object System.Drawing.Font '맑은 고딕',40,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$fmt=New-Object System.Drawing.StringFormat
$fmt.Alignment='Center';$fmt.LineAlignment='Center'
function Text($s,$x,$y,$w,$h,$font){
 $shadow=New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(120,6,34,60))
 $g.DrawString($s,$font,$shadow,[System.Drawing.RectangleF]::new(($x+2),($y+4),$w,$h),$fmt)
 $g.DrawString($s,$font,$ink,[System.Drawing.RectangleF]::new($x,$y,$w,$h),$fmt);$shadow.Dispose()
}
function Section($s,$y){
 $ink.Color=[System.Drawing.ColorTranslator]::FromHtml('#17334A')
 $fmt.Alignment='Near'
 Text $s 140 $y 1600 80 $head
 $fmt.Alignment='Center'
 $ink.Color=[System.Drawing.Color]::White
}
function Round($x,$y,$w,$h,$r){
 $p=New-Object System.Drawing.Drawing2D.GraphicsPath
 $d=$r*2;$p.AddArc($x,$y,$d,$d,180,90);$p.AddArc(($x+$w-$d),$y,$d,$d,270,90);$p.AddArc(($x+$w-$d),($y+$h-$d),$d,$d,0,90);$p.AddArc($x,($y+$h-$d),$d,$d,90,90);$p.CloseFigure();return $p
}
function Node($x,$y,$w,$h,$heading,$desc,$tech,$color){
 $outline=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#072E50')),6
 $shadow=Round $x ($y+17) $w $h 30
 $g.FillPath([System.Drawing.Brushes]::MidnightBlue,$shadow)
 $path=Round $x $y $w $h 30
 $brush=New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.Rectangle]::new($x,$y,$w,$h)),([System.Drawing.ColorTranslator]::FromHtml('#149AE3')),([System.Drawing.ColorTranslator]::FromHtml('#075D9F')),90
 $g.FillPath($brush,$path);$g.DrawPath($outline,$path)
 $gloss=New-Object System.Drawing.Pen ([System.Drawing.Color]::FromArgb(120,173,241,255)),4
 $g.DrawLine($gloss,($x+35),($y+10),($x+$w-35),($y+10))
 $gold=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#FFD32B')),7
 $g.DrawLine($gold,($x+45),($y+122),($x+$w-45),($y+122))
 Text $heading ($x+15) ($y+20) ($w-30) 95 $head
 Text $desc ($x+25) ($y+125) ($w-50) ($h-200) $body
 Text $tech ($x+15) ($y+$h-70) ($w-30) 55 $small
 $outline.Dispose();$brush.Dispose();$shadow.Dispose();$path.Dispose();$gloss.Dispose();$gold.Dispose()
}
$arrow=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#FFE24E')),12
$arrow.CustomEndCap=New-Object System.Drawing.Drawing2D.AdjustableArrowCap 7,8
$line=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#FFE24E')),12
function Link($x1,$y1,$x2,$y2){$g.DrawLine($arrow,$x1,$y1,$x2,$y2)}
$banner=Round 560 65 2480 180 35
$bannerBrush=New-Object System.Drawing.Drawing2D.LinearGradientBrush ([System.Drawing.Rectangle]::new(560,65,2480,180)),([System.Drawing.ColorTranslator]::FromHtml('#FFE333')),([System.Drawing.ColorTranslator]::FromHtml('#EAAF0A')),90
$bannerPen=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#073455')),7
$g.FillPath($bannerBrush,$banner);$g.DrawPath($bannerPen,$banner)
$ink.Color=[System.Drawing.ColorTranslator]::FromHtml('#123B5A')
Text 'AI 디펜더스 시스템 구성도' 100 85 3400 130 $title
$ink.Color=[System.Drawing.Color]::White
$banner.Dispose();$bannerBrush.Dispose();$bannerPen.Dispose()

$g.TranslateTransform(0,-80)
Section '1   데이터 제작과 AI 학습' 385
Node 400 520 1200 400 '데이터셋 제작' "게임 속 양·늑대 사진을 만들고`n각 대상의 이름과 위치를 표시" '데이터셋 팩토리 · 사진 + 정답 표시' '#258AAF'
Node 2000 520 1200 400 'AI 학습' "사진과 정답으로 양·늑대의 특징 학습`n학습한 모델을 인식 서버에 적용" 'YOLO · PyTorch · 학습 모델 (.pt)' '#7861AD'
Link 1650 720 1930 720
$g.DrawLine($line,2600,955,2600,1040)
$g.DrawLine($line,2600,1040,1800,1040)
Link 1800 1040 1800 1230
$ink.Color=[System.Drawing.ColorTranslator]::FromHtml('#17334A')
Text '학습한 모델 사용' 2050 955 550 65 $small
$ink.Color=[System.Drawing.Color]::White
Section '2   실시간 인식과 기지 방어' 1100
Node 140 1280 1000 440 '게임 카메라' "전투 장면을 촬영해`n인식 서버로 사진 전송" 'Unity · CCTV' '#258AAF'
Node 1300 1280 1000 440 'AI 인식 · 피아식별' "사진 속 대상의 종류와 위치 확인`n실제 대상과 연결해 아군·적 구별" 'Python · FastAPI · YOLO → Unity' '#319984'
Node 2460 1280 1000 440 '포탑 자동 공격' "구별한 적을 표적으로 지정`n포탑이 조준하고 공격해 기지 방어" 'Unity · 포탑 시스템' '#C18A35'
Link 1170 1500 1255 1500
Link 2330 1500 2415 1500
Link 2960 1765 2960 1940
$ink.Color=[System.Drawing.ColorTranslator]::FromHtml('#17334A')
Text '전투 기록' 3020 1780 370 70 $small
$ink.Color=[System.Drawing.Color]::White
Section '3   결과 확인과 성장' 1810
Node 400 1990 2800 300 '전투 결과 · 영구 연구' "점수·식별 정확도 확인 → 연구 포인트 획득 → 포탑 성능 강화" '연구 효과는 다음 전투에도 적용 · 진행 정보는 내 컴퓨터에 저장' '#527B96'
$target=Join-Path $PSScriptRoot '../Submission/Originals/02_SystemArchitecture_GameTheme_3x2.png'
$bmp.Save([IO.Path]::GetFullPath($target),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$ink.Dispose();$title.Dispose();$head.Dispose();$body.Dispose();$small.Dispose();$fmt.Dispose();$arrow.Dispose();$line.Dispose()
