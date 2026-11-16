Add-Type -AssemblyName System.Drawing
$bmp=New-Object System.Drawing.Bitmap 3600,2400
$bmp.SetResolution(300,300)
$g=[System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode='AntiAlias';$g.TextRenderingHint='AntiAliasGridFit'
$g.Clear([System.Drawing.ColorTranslator]::FromHtml('#F5F9FC'))
$ink=New-Object System.Drawing.SolidBrush ([System.Drawing.ColorTranslator]::FromHtml('#17334A'))
$title=New-Object System.Drawing.Font '맑은 고딕',112,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$head=New-Object System.Drawing.Font '맑은 고딕',82,([System.Drawing.FontStyle]::Bold),([System.Drawing.GraphicsUnit]::Pixel)
$body=New-Object System.Drawing.Font '맑은 고딕',62,([System.Drawing.FontStyle]::Regular),([System.Drawing.GraphicsUnit]::Pixel)
$fmt=New-Object System.Drawing.StringFormat
$fmt.Alignment='Center';$fmt.LineAlignment='Center'
function Text($s,$x,$y,$w,$h,$font){$g.DrawString($s,$font,$ink,[System.Drawing.RectangleF]::new($x,$y,$w,$h),$fmt)}
function Card($x,$y,$n,$heading,$description,$color){
 $c=[System.Drawing.ColorTranslator]::FromHtml($color)
 $brush=New-Object System.Drawing.SolidBrush $c
 $pen=New-Object System.Drawing.Pen $c,7
 $g.FillRectangle([System.Drawing.Brushes]::White,$x,$y,1400,650)
 $g.DrawRectangle($pen,$x,$y,1400,650)
 $g.FillEllipse($brush,($x+590),($y+45),220,220)
 $g.DrawString($n,$title,[System.Drawing.Brushes]::White,[System.Drawing.RectangleF]::new(($x+590),($y+45),220,220),$fmt)
 Text $heading ($x+30) ($y+295) 1340 115 $head
 Text $description ($x+60) ($y+430) 1280 170 $body
 $brush.Dispose();$pen.Dispose()
}
$arrow=New-Object System.Drawing.Pen ([System.Drawing.ColorTranslator]::FromHtml('#59849E')),18
$arrow.CustomEndCap=New-Object System.Drawing.Drawing2D.AdjustableArrowCap 9,10
Text 'AI 디펜더스는 이렇게 움직여요!' 100 85 3400 160 $title
Text 'AI가 아군과 적을 구별하고, 포탑이 기지를 지켜요.' 100 265 3400 100 $body
Card 200 480 '1' '사진을 모아요' "양과 늑대 사진을 만들고`n무엇이 양인지, 늑대인지 알려줘요." '#208BB0'
Card 2000 480 '2' 'AI를 가르쳐요' "AI가 사진을 보며`n양과 늑대의 모습을 배워요." '#7662B4'
Card 2000 1360 '3' '아군과 적을 찾아요' "게임 속 카메라 영상을 보고`nAI가 양과 늑대를 구별해요." '#319984'
Card 200 1360 '4' '포탑이 기지를 지켜요' "구별한 결과를 포탑에 알려줘요.`n포탑은 적을 겨냥해 공격해요." '#CC8930'
$g.DrawLine($arrow,1660,805,1925,805)
$g.DrawLine($arrow,2700,1180,2700,1285)
$g.DrawLine($arrow,1940,1685,1675,1685)
Text '전투가 끝나면 결과를 확인하고, 연구 포인트로 포탑을 더 강하게 만들어요.' 160 2140 3280 150 $body
$target=Join-Path $PSScriptRoot '../Submission/Originals/02_SystemArchitecture_Easy.png'
$bmp.Save([IO.Path]::GetFullPath($target),[System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose();$bmp.Dispose();$ink.Dispose();$title.Dispose();$head.Dispose();$body.Dispose();$fmt.Dispose();$arrow.Dispose()
