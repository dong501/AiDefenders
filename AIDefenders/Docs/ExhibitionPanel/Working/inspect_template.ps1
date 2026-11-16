$ErrorActionPreference = 'Stop'
$ppt = New-Object -ComObject PowerPoint.Application
$source = Join-Path $PSScriptRoot 'template.pptx'
$deck = $ppt.Presentations.Open($source, -1, 0, 0)
try {
    for ($i = 1; $i -le $deck.Slides.Count; $i++) {
        $outPath = Join-Path $PSScriptRoot ('Template_' + $i + '.png')
        $deck.Slides.Item($i).Export($outPath, 'PNG', 1400, 1685)
    }
    Write-Output ('Slides=' + $deck.Slides.Count)
} finally {
    $deck.Close()
    $ppt.Quit()
}
