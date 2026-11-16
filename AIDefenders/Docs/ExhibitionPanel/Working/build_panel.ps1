$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$content = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'content.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$output = Join-Path (Split-Path $PSScriptRoot) 'Submission'
$preview = Join-Path (Split-Path $PSScriptRoot) 'Preview'
New-Item -ItemType Directory -Force $preview | Out-Null
$app = New-Object -ComObject PowerPoint.Application
$deck = $null
function Mm([double]$value) { return [single]($value * 72 / 25.4) }
function TextBox($slide, $name, $text, $x, $y, $w, $h, $size, $heading = $false, $align = 1) {
    $shape = $slide.Shapes.AddTextbox(1, (Mm $x), (Mm $y), (Mm $w), (Mm $h))
    $shape.Name = $name
    $shape.TextFrame.MarginLeft = 0
    $shape.TextFrame.MarginRight = 0
    $shape.TextFrame.MarginTop = 0
    $shape.TextFrame.MarginBottom = 0
    $shape.TextFrame.WordWrap = -1
    $shape.TextFrame.AutoSize = 0
    $range = $shape.TextFrame.TextRange
    $range.Text = $text.Replace("`n", "`r")
    $font = if ($heading) { 'HY견고딕' } else { '맑은 고딕' }
    $range.Font.Name = $font
    $range.Font.NameFarEast = $font
    $range.Font.Size = $size
    $range.Font.Bold = if ($heading) { -1 } else { 0 }
    $range.Font.Color.RGB = 0
    $range.ParagraphFormat.Alignment = $align
    $range.ParagraphFormat.SpaceBefore = 0
    $range.ParagraphFormat.SpaceAfter = 0
    $range.ParagraphFormat.LineRuleWithin = -1
    $range.ParagraphFormat.SpaceWithin = 1.12
    $shape.TextFrame2.AutoSize = 0
    $shape.Width = Mm $w
    $shape.Height = Mm $h
    return $shape
}
function Box($slide, $name, $x, $y, $w, $h) {
    $shape = $slide.Shapes.AddShape(5, (Mm $x), (Mm $y), (Mm $w), (Mm $h))
    $shape.Name = $name
    $shape.Fill.ForeColor.RGB = 16777215
    $shape.Line.ForeColor.RGB = 10526880
    $shape.Line.Weight = .8
    return $shape
}
function Photo($slide, $name, $file, $x, $y, $w, $h) {
    $path = Join-Path $output ('Originals\' + $file)
    $bitmap = [System.Drawing.Image]::FromFile($path)
    try { $ratio = $bitmap.Width / [double]$bitmap.Height } finally { $bitmap.Dispose() }
    $pw = $w
    $ph = $pw / $ratio
    if ($ph -gt $h) { $ph = $h; $pw = $ph * $ratio }
    $picture = $slide.Shapes.AddPicture($path, 0, -1, (Mm ($x + ($w-$pw)/2)), (Mm ($y + ($h-$ph)/2)), (Mm $pw), (Mm $ph))
    $picture.Name = $name
    $picture.Line.Visible = -1
    $picture.Line.ForeColor.RGB = 12632256
    $picture.Line.Weight = .5
}
try {
    $deck = $app.Presentations.Open((Join-Path $PSScriptRoot 'template.pptx'), 0, 0, 0)
    $deck.Slides.Item(1).Delete()
    $deck.Slides.Item(1).Delete()
    $slide = $deck.Slides.Item(1)
    for ($i = $slide.Shapes.Count; $i -ge 1; $i--) { $slide.Shapes.Item($i).Delete() }
    $deck.PageSetup.SlideWidth = Mm 594
    $deck.PageSetup.SlideHeight = Mm 719
    $slide.FollowMasterBackground = 0
    $slide.Background.Fill.Solid()
    $slide.Background.Fill.ForeColor.RGB = 16777215
    TextBox $slide 'ProjectTitle' $content.title 40 30 514 20 44 $true | Out-Null
    TextBox $slide 'TeamAndAdvisor' $content.meta 40 53 514 15 28 | Out-Null
    TextBox $slide 'OverviewHeading' $content.overviewTitle 40 80 247 20 44 $true | Out-Null
    TextBox $slide 'TechnologyHeading' $content.techTitle 307 80 247 20 44 $true | Out-Null
    Box $slide 'OverviewBox' 40 105 247 128 | Out-Null
    Box $slide 'TechnologyBox' 307 105 247 128 | Out-Null
    TextBox $slide 'OverviewBody' $content.overview 48 114 231 112 30 | Out-Null
    TextBox $slide 'TechnologyBody' $content.tech 315 114 231 112 28 | Out-Null
    TextBox $slide 'PhotosHeading' $content.photoTitle 40 248 514 20 44 $true | Out-Null
    Photo $slide 'GrasslandTurretPhoto' '01_GrasslandTurret.png' 40 276 514 139
    TextBox $slide 'GrasslandCaption' $content.captions[0] 40 420 514 15 28 $false 2 | Out-Null
    Photo $slide 'ArchitecturePhoto' '02_SystemArchitecture.png' 40 445 514 139
    TextBox $slide 'ArchitectureCaption' $content.captions[1] 40 589 514 15 28 $false 2 | Out-Null
    TextBox $slide 'ConclusionHeading' $content.conclusionTitle 40 620 514 20 44 $true | Out-Null
    TextBox $slide 'ConclusionBody' $content.conclusion 40 645 514 44 30 | Out-Null
    $findings = @()
    foreach ($shape in $slide.Shapes) {
        if ($shape.HasTextFrame -and $shape.TextFrame.HasText) {
            $bound = $shape.TextFrame.TextRange.BoundHeight
            $findings += [pscustomobject]@{Name=$shape.Name; Font=$shape.TextFrame.TextRange.Font.Name; Size=$shape.TextFrame.TextRange.Font.Size; BoundHeight=[math]::Round($bound,1); BoxHeight=[math]::Round($shape.Height,1); Overflow=($bound -gt $shape.Height + 1)}
        }
    }
    $findings | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $preview 'layout_check.json') -Encoding UTF8
    if (@($findings | Where-Object Overflow).Count -gt 0) { throw 'Some text exceeds its box; inspect layout_check.json.' }
    $deck.SaveAs((Join-Path $output ($content.fileName + '.pptx')), 24, -1)
    $deck.SaveAs((Join-Path $preview ($content.fileName + '.pdf')), 32)
    $slide.Export((Join-Path $preview 'PanelPreview.png'), 'PNG', 1980, 2397)
    Write-Output 'Panel, PDF and preview exported.'
} finally {
    if ($null -ne $deck) { $deck.Close() }
    $app.Quit()
}
