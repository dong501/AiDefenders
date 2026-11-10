$ErrorActionPreference='Stop'
$app=New-Object -ComObject PowerPoint.Application
$deck=$null
try{
 $deck=$app.Presentations.Open('C:\Users\user\Downloads\AI 디펜더스_소프트웨어학과_동현우, 유기현.pptx',-1,0,0)
 $slide=$deck.Slides.Item(1)
 $slide.Export((Join-Path $PSScriptRoot 'UserPanel.png'),'PNG',1800,2200)
 $rows=@()
 foreach($s in $slide.Shapes){$t='';if($s.HasTextFrame -and $s.TextFrame.HasText){$t=$s.TextFrame.TextRange.Text};$rows += [pscustomobject]@{Id=$s.Id;Name=$s.Name;Type=$s.Type;X=$s.Left;Y=$s.Top;W=$s.Width;H=$s.Height;Text=$t}}
 $rows | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'user_panel_shapes.json') -Encoding UTF8
 Write-Output ('Slide dimensions: '+$deck.PageSetup.SlideWidth+' x '+$deck.PageSetup.SlideHeight)
}finally{if($deck){$deck.Close()};$app.Quit()}
