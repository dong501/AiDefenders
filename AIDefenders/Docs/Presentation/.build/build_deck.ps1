$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$root = Split-Path -Parent $PSScriptRoot
$fso = New-Object -ComObject Scripting.FileSystemObject
$build = $fso.GetFolder($PSScriptRoot).ShortPath
$assets = Join-Path $build 'assets'
$source = $fso.GetFile((Join-Path $build 'source.pptx')).ShortPath
$output = Join-Path $root 'AI_Defenders_개발진행보고.pptx'
$pdf = Join-Path $root 'AI_Defenders_개발진행보고.pdf'
$renders = Join-Path $build 'rendered'
New-Item -ItemType Directory -Force -Path $renders | Out-Null
function RGB([int]$r,[int]$g,[int]$b) { return $r + 256*$g + 65536*$b }
$green=RGB 116 160 80; $dark=RGB 80 115 53; $ink=RGB 25 32 25; $muted=RGB 91 103 85; $pale=RGB 239 244 232; $white=16777215
function Rect($s,$x,$y,$w,$h,$color) {
    $a=$s.Shapes.AddShape(1,$x,$y,$w,$h);$a.Fill.ForeColor.RGB=$color;$a.Line.Visible=0;return $a
}
function Text($s,$txt,$x,$y,$w,$h,$size=24,$color=$ink,$bold=$false,$align=1) {
    $a=$s.Shapes.AddTextbox(1,$x,$y,$w,$h);$a.Name='Text_'+$s.Shapes.Count
    $tf=$a.TextFrame;$tf.MarginLeft=0;$tf.MarginRight=0;$tf.MarginTop=0;$tf.MarginBottom=0;$tf.WordWrap=-1;$tf.AutoSize=0
    $a.Width=$w;$a.Height=$h
    $tr=$tf.TextRange;$tr.Text=$txt;$tr.Font.Name='맑은 고딕';$tr.Font.NameFarEast='맑은 고딕';$tr.Font.Size=$size;$tr.Font.Color.RGB=$color;$tr.Font.Bold=[int]$bold*-1
    $tr.ParagraphFormat.Alignment=$align;$tr.ParagraphFormat.SpaceAfter=8
    return $a
}
function Note($s,$txt) { $s.NotesPage.Shapes.Placeholders.Item(2).TextFrame.TextRange.Text=$txt }
function Photo($s,$name,$x,$y,$w,$h,$front=$false) {
    $path=Join-Path $assets $name
    $im=[Drawing.Image]::FromFile($path);$iw=$im.Width;$ih=$im.Height;$im.Dispose()
    $p=$s.Shapes.AddPicture($path,0,-1,0,0,-1,-1)
    if($front){
        $pw=$p.Width;$ph=$p.Height
        $p.PictureFormat.CropLeft=$pw*(45/1500);$p.PictureFormat.CropRight=$pw*(755/1500)
        $p.PictureFormat.CropTop=$ph*(170/850);$p.PictureFormat.CropBottom=$ph*(70/850)
        $iw=700;$ih=610
    }
    $factor=[Math]::Min($w/$iw,$h/$ih);$p.LockAspectRatio=0;$p.Width=[single]($iw*$factor);$p.Height=[single]($ih*$factor)
    $p.Left=[single]($x+($w-$p.Width)/2);$p.Top=[single]($y+($h-$p.Height)/2)
    return $p
}
function Header($title,$sub) {
    $s=$deck.Slides.Add($deck.Slides.Count+1,12);$s.DisplayMasterShapes=0;$s.FollowMasterBackground=0;$s.Background.Fill.ForeColor.RGB=$green
    $null=Rect $s 91 91 1264 135 $dark;$null=Rect $s 81 81 1264 135 $white
    $null=Text $s $title 120 108 1185 61 37 $ink $false
    $null=Text $s $sub 123 176 1160 31 19 $dark
    $null=Rect $s 91 268 1264 468 $dark;$null=Rect $s 81 258 1264 468 $white
    $null=Text $s 'AI DEFENDERS  /  개발 진행 보고' 85 758 1100 26 13 $white
    $null=Text $s ('{0:00}' -f $s.SlideIndex) 1260 752 85 35 19 $white $false 3
    return $s
}
function PixelBar($s,$x,$y,$fill) {
    $null=Rect $s ($x+32) ($y+6) 172 20 $white
    if($fill -gt 0){$null=Rect $s ($x+32) ($y+6) $fill 20 (RGB 33 92 201)}
    $null=Rect $s ($x+26) ($y+6) 7 6 0;$null=Rect $s ($x+198) ($y+20) 7 6 0
    $pixels=@('01100110','11111111','11111111','01111110','00111100','00011000')
    for($r=0;$r -lt 6;$r++){for($c=0;$c -lt 8;$c++){if($pixels[$r][$c] -eq '1'){$null=Rect $s ($x+$c*5) ($y+$r*5) 5 5 (RGB 218 62 40)}}}
}
function SmallLabel($s,$txt,$x,$y,$w) { $null=Text $s $txt $x $y $w 34 23 $dark $true }
function ModelSlide($title,$sub,$prefix,$captions,$detail,$note) {
    $s=Header $title $sub
    for($i=0;$i -lt 3;$i++){
        $x=112+$i*407
        $null=Rect $s $x 285 382 318 (RGB 231 237 241)
        $null=Photo $s ($prefix+' '+($i+1)+'단계.png') ($x+8) 295 366 285 $true
        $null=Text $s (($i+1).ToString()+'단계') ($x+6) 615 370 36 25 $dark $true 2
        $null=Text $s $captions[$i] ($x+2) 656 378 39 20 $ink $false 2
        if($i -lt 2){$null=Text $s '→' ($x+377) 414 35 55 27 $dark $true 2}
    }
    Note $s ($note+"`r`n"+$detail)
    return $s
}
$ppt=New-Object -ComObject PowerPoint.Application
try {
    $deck=$ppt.Presentations.Open($source,0,0,0)
    while($deck.Slides.Count -gt 0){$deck.Slides.Item(1).Delete()}
    $deck.PageSetup.SlideWidth=1440;$deck.PageSetup.SlideHeight=810

    # 01 Cover — retain the original black/white/green and pixel HUD motif.
    $s=$deck.Slides.Add(1,12);$s.DisplayMasterShapes=0;$s.FollowMasterBackground=0;$s.Background.Fill.ForeColor.RGB=0
    PixelBar $s 81 86 0;PixelBar $s 1140 86 135
    $null=Rect $s 91 220 1264 364 $dark;$null=Rect $s 81 210 1264 364 $white
    $null=Text $s '유니티 3D 엔진을 활용한' 130 248 1160 50 29 $ink $false 2
    $null=Text $s '지능형 타워 디펜스' 130 322 1160 92 57 $ink $false 2
    $null=Text $s 'AI DEFENDERS  /  개발 진행 보고' 130 445 1160 44 29 $dark $false 2
    $null=Text $s '스테이지 확장 · 포탑 모델링 · 영구 연구 · 결과 UI' 125 516 1170 35 21 $muted $false 2
    $null=Text $s 'CAPSTONE PROJECT' 120 648 1200 36 21 $white $false 2
    Note $s '이번 발표는 최근 진행한 콘텐츠와 화면 구현을 중심으로 설명합니다. 초원·사막·도시 스테이지, 포탑 단계별 모델링, 영구 연구와 결과 화면 순서로 소개합니다.'

    # 02 Overview.
    $s=Header '주요 진행 사항' '맵 콘텐츠와 성장 시스템, 전투 피드백 화면을 확장'
    $cards=@(@('01','스테이지 확장','초원 낮·저녁·밤 / 사막 / 도시','3개 챕터 · 제작된 스테이지 5개'),@('02','포탑 업그레이드 모델링','정밀 사격 / 기관총 / 미사일 / 레이저','4종 포탑의 1·2·3단계 모델링 자료'),@('03','포탑 연구소','포탑 선택 · 연구 해금 · 성능 확인','연구 포인트를 사용하는 영구 업그레이드'),@('04','결과 화면','점수 · 처치 / 통과 · 식별 정확도','최종 평가와 연구 포인트 획득량 표시'))
    for($i=0;$i -lt 4;$i++){
      $x=112+($i%2)*610;$y=284+[int][Math]::Floor($i/2)*207
      $null=Rect $s $x $y 585 185 $pale
      $null=Text $s $cards[$i][0] ($x+20) ($y+17) 60 45 28 $dark $true
      $null=Text $s $cards[$i][1] ($x+91) ($y+18) 470 42 27 $ink $true
      $null=Text $s $cards[$i][2] ($x+24) ($y+82) 535 39 22 $ink
      $null=Text $s $cards[$i][3] ($x+24) ($y+130) 535 30 18 $muted
    }
    Note $s '작업은 네 가지 영역으로 정리했습니다. 스테이지는 현재 다섯 개가 제작되어 있으며, 포탑은 네 종류의 단계별 모델링 자료와 게임 적용 상태를 구분해 소개합니다.'

    # 03 Campaign structure, editable diagram.
    $s=Header '챕터 선택 → 스테이지 선택' '메인 화면에서 테마를 고르고, 해당 챕터의 스테이지로 진입'
    $null=Text $s '메인 화면' 123 283 240 43 27 $ink $true
    $null=Text $s '→' 345 284 60 43 30 $dark $true 2
    $null=Text $s '챕터 선택' 419 283 300 43 27 $ink $true
    $null=Text $s '→' 725 284 60 43 30 $dark $true 2
    $null=Text $s '스테이지 선택' 805 283 400 43 27 $ink $true
    $chapters=@(@('CHAPTER 1','초원','grass_day_1.png','낮  /  저녁  /  밤'),@('CHAPTER 2','사막','desert_road.png','낮 스테이지'),@('CHAPTER 3','도시','city_road.png','도시 스테이지'))
    for($i=0;$i -lt 3;$i++){
      $x=112+$i*407;$null=Rect $s $x 349 382 307 $pale
      $null=Text $s $chapters[$i][0] ($x+17) 361 345 25 15 $dark $true
      $null=Text $s $chapters[$i][1] ($x+17) 390 345 38 28 $ink $true
      $null=Photo $s $chapters[$i][2] ($x+14) 442 354 154
      $null=Text $s $chapters[$i][3] ($x+14) 612 354 31 21 $dark $false 2
    }
    $null=Text $s '현재 제작된 5개 스테이지는 모두 선택 가능' 123 677 1170 32 20 $dark
    Note $s '메인 화면에서 먼저 챕터를 선택하고, 다음 화면에서 스테이지를 선택하는 구조입니다. 초원에는 낮·저녁·밤 세 스테이지, 사막과 도시에는 각각 한 개의 제작된 스테이지가 있습니다. 테스트를 위해 모두 선택할 수 있습니다.'

    # 04 Grass day/dusk/night comparison.
    $s=Header '초원 챕터 — 낮 · 저녁 · 밤' '같은 초원 지형에 시간대별 하늘과 조명을 적용'
    $times=@(@('낮','grass_day_1.png','밝은 하늘과 자연광'),@('저녁','grass_dusk.png','노을과 따뜻한 색감'),@('밤','grass_night.png','밤하늘과 차가운 색감'))
    for($i=0;$i -lt 3;$i++){
      $x=112+$i*407;$null=Text $s $times[$i][0] $x 282 382 42 28 $dark $true 2
      $null=Photo $s $times[$i][1] $x 338 382 245
      $null=Text $s $times[$i][2] $x 606 382 37 21 $ink $false 2
    }
    $null=Text $s '지형의 일관성을 유지하면서 시간대에 따른 분위기를 구분' 123 672 1180 34 22 $dark
    Note $s '같은 구도의 실제 씬을 사용해 시간대 차이를 비교했습니다. 지형과 도로 구성은 유지하고, 낮·저녁·밤의 하늘과 광원 색으로 서로 다른 분위기를 만들었습니다.'

    # 05 Ranch.
    $s=Header '초원 환경 오브젝트 보강' '호수와 숲, 울타리 안 목장으로 초원 테마를 구체화'
    $null=Photo $s 'ranch.png' 111 286 815 392
    SmallLabel $s '목장 구성' 965 303 310
    $null=Text $s "울타리 안 축사와 양 배치`r`n건초·급수대 등 소품 추가`r`n호숫가와 주변 숲에 연결" 965 357 333 181 23 $ink
    SmallLabel $s '적과 보호 대상의 구분' 965 557 333
    $null=Text $s "늑대는 적 표적`r`n양은 보호 대상으로 구분" 965 608 333 70 22 $ink
    Note $s '초원에서는 호수와 숲이라는 기존 배경에 목장을 추가했습니다. 울타리 안에 축사, 양, 건초와 급수대를 배치해 보호 대상이 있는 생활 공간으로 보이도록 구성했습니다.'

    # 06 Desert.
    $s=Header '사막 스테이지 제작' '모래 지형과 굽이치는 진입로, 전초기지 형태의 방어 목표'
    $null=Photo $s 'desert_road.png' 111 281 777 350
    $null=Text $s '모래언덕 · 바위 · 선인장 · 사막 진입로' 123 648 753 37 22 $dark
    $null=Photo $s 'desert_hq.png' 919 282 392 224
    SmallLabel $s '방어 목표 주변' 936 526 360
    $null=Text $s "천막·지프·감시탑 배치`r`n사막에 맞는 방호벽 구성`r`n스폰과 거점 사이 거리 조정" 936 575 373 122 21 $ink
    Note $s '사막은 모래언덕과 바위, 선인장 등으로 초원과 다른 환경을 만들었습니다. 방어 목표 주변에는 천막과 지프, 감시탑과 방호벽을 배치해 전초기지의 인상을 주었습니다. 진입 경로와 스폰 거리는 플레이 동선을 보며 조정했습니다.'

    # 07 City.
    $s=Header '도시 스테이지 제작' '좀비 전투 테마에 맞춘 도로와 대피소형 방어 거점'
    $null=Photo $s 'city_road.png' 111 291 589 294
    $null=Photo $s 'city_shelter.png' 722 291 589 294
    SmallLabel $s '도시 도로와 주변 환경' 123 604 565
    SmallLabel $s '대피소형 방어 거점' 735 604 565
    $null=Text $s '교차로·횡단보도·가로수·차량·건물' 123 653 565 40 20 $ink
    $null=Text $s '대피소 표지·응급 공간·차단 시설·옅은 안개' 735 653 565 40 20 $ink
    Note $s '도시는 교차로와 횡단보도를 중심으로 도로망을 구성했습니다. 방어 목표는 대피소로 표현했고, 차량과 차단 시설, 응급 공간을 더했습니다. 옅은 안개로 좀비가 등장하는 도시 분위기를 보강했습니다.'

    # 08 Modeling common direction.
    $s=Header '포탑 업그레이드 모델링' '4종 포탑의 실루엣과 단계별 확장 방향 정리'
    $models=@(@('정밀 사격','정밀 사격 1단계.png','사각 포신'),@('기관총','기관총 1단계.png','다연장 총열'),@('미사일','미사일 1단계.png','미사일 발사관'),@('레이저','레이저 1단계.png','청록색 에너지 코어'))
    for($i=0;$i -lt 4;$i++){
      $x=112+$i*305;$null=Rect $s $x 282 281 295 (RGB 231 237 241)
      $null=Photo $s $models[$i][1] ($x+7) 289 267 258 $true
      $null=Text $s $models[$i][0] $x 596 281 37 25 $dark $true 2
      $null=Text $s $models[$i][2] $x 642 281 34 19 $ink $false 2
    }
    $null=Text $s '공통 색상: 짙은 청색 + 금색 + 청록색 발광 포인트' 123 684 1180 28 18 $muted
    Note $s '포탑은 같은 색상 체계를 공유하되 무기별 핵심 부품으로 구분했습니다. 정밀 사격의 사각 포신, 기관총의 다연장 총열, 미사일의 발사관, 레이저의 에너지 코어가 구분점입니다. 첨부 이미지 안의 영문 제목보다 파일명을 기준으로 분류했습니다.'

    # 09–12: all supplied modeling sheets, clearly distinguished from game integration.
    $null=ModelSlide '기관총 포탑 — 단계별 모델링' '다연장 총열을 확장하며 화력 증가를 시각적으로 표현' '기관총' @('3연장 단일 포드','3연장 포드 2개','6연장 포드 3개') '게임 적용: 새 기관총 모델 1·2·3단계 연결 확인.' '1단계는 세 개의 총열, 2단계는 두 개의 포드, 3단계는 세 개의 회전 총열 포드로 확장됩니다. 단계가 올라갈수록 총열과 지지 구조가 커지는 방향으로 정리했습니다.'
    $null=ModelSlide '정밀 사격 포탑 — 단계별 모델링' '사각 포신에서 다중 포신 구조로 확장' '정밀 사격' @('단일 사각 포신','쌍열 포신','4개 포신과 냉각부') '게임 적용: 기존 기관총의 사각 포신 스킨을 정밀 사격으로 이동. 제공된 3단계 이미지는 모델링 자료 기준.' '정밀 사격은 파일명을 기준으로 구분했습니다. 기존 정밀 사격의 외형을 제거하고 기관총에 사용하던 사각 포신 외형을 옮겼습니다. 이 슬라이드는 제공된 1·2·3단계 모델링 이미지를 비교합니다.'
    $null=ModelSlide '미사일 포탑 — 단계별 모델링' '발사관과 탑재 미사일 수를 늘리는 구조' '미사일' @('단일 미사일 발사관','쌍발 발사관','양쪽 다연장 발사관') '게임 적용: 첨부 이미지와 일치하는 1단계 모델 연결. 2·3단계는 제공된 모델링 자료.' '미사일은 단일 발사관에서 쌍발, 양쪽 다연장 발사관으로 확장됩니다. 1단계 모델은 게임에 적용했고, 상위 단계는 첨부된 모델링 자료를 중심으로 소개합니다.'
    $null=ModelSlide '레이저 포탑 — 단계별 모델링' '발광 코어와 냉각 장치를 확장해 에너지 무기 특성을 강조' '레이저' @('청록색 발광 코어','상부 냉각부 확장','에너지 셀과 보조 장치') '게임 적용: 레이저 V2 기반 1단계 모델 연결. 2·3단계는 제공된 모델링 자료.' '레이저는 청록색 발광 코어를 유지하면서 냉각부와 에너지 셀을 확장하는 방향입니다. 이미지에 표시된 V2는 모델 버전이며, 게임 분류는 파일명에 따라 1단계로 정리했습니다.'

    # 13 Lab.
    $s=Header '포탑 연구소 — 영구 업그레이드' '메인 화면 → Turret Lab → 포탑 선택 → 연구 해금'
    $null=Photo $s 'lab.png' 111 285 858 394
    SmallLabel $s '연구 목록에서 해금' 996 299 306
    $null=Text $s "연구 포인트와 선행 조건 확인`r`n해금한 연구는 이후 전투에 적용`r`n연구 적용 성능을 수치로 확인" 996 350 312 155 21 $ink
    $null=Rect $s 990 524 320 162 $pale
    $null=Text $s '연구 예시' 1006 537 288 31 22 $dark $true
    $null=Text $s "정밀 사격: 탄속·사거리·관통`r`n기관총: 탄퍼짐·재장전 개선" 1006 581 287 85 19 $ink
    $null=Text $s '1·2·3단계 버튼은 외형 미리보기이며, 영구 연구는 아래 연구 목록에서 진행' 123 689 1180 26 17 $muted
    Note $s '포탑 연구소에서는 오른쪽 포탑 목록을 선택한 뒤, 아래 연구 목록에서 포인트를 사용해 연구를 해금합니다. 선행 조건을 가진 연구도 있으며, 해금 내역은 저장되어 다음 전투의 성능에 적용됩니다. 1·2·3단계 버튼은 미리보기이고 전투 중 단계 업그레이드와 영구 연구는 구분됩니다. 연구는 현재 정밀 사격과 기관총을 중심으로 구성되어 있습니다.'

    # 14 Results.
    $s=Header '스테이지 결과 화면' '전투 기록과 피아식별 성능, 연구 포인트를 한 화면에서 확인'
    $null=Photo $s 'result.png' 111 283 852 403
    SmallLabel $s '전투 결과 요약' 994 298 306
    $null=Text $s "성공·실패 및 평가 등급`r`n획득 점수와 최종 점수`r`n적·아군의 처치 및 통과 수" 994 346 315 143 22 $ink
    SmallLabel $s '피아식별과 보상' 994 517 306
    $null=Text $s "정답·오식별·미식별과 정확도`r`n획득 / 보유 연구 포인트`r`n메인 메뉴 복귀" 994 566 315 117 21 $ink
    $null=Text $s '실패 상태의 결과 화면 예시 · 식별 정확도와 연구 포인트 확인' 123 690 1180 25 17 $muted
    Note $s '결과 화면은 단순한 성공 여부뿐 아니라 점수, 처치·통과 내역, 식별 정확도와 연구 포인트를 함께 보여줍니다. 식별 정확도는 정답 수를 정답·오식별·미식별 합계로 나눈 값입니다. 화면은 실패 상태의 UI 표시 예시이며, 숫자는 실제 성능 측정 결과로 제시하지 않습니다.'

    # 15 Gameplay cycle.
    $s=Header '전투와 성장의 흐름 연결' '스테이지 선택부터 결과 확인과 영구 연구까지'
    $steps=@(@('01','챕터 / 스테이지 선택','초원 · 사막 · 도시'),@('02','전투 및 포탑 강화','배치 · 단계 업그레이드'),@('03','결과 확인','점수 · 식별 정확도'),@('04','연구 포인트 사용','영구 성능 업그레이드'))
    for($i=0;$i -lt 4;$i++){
      $x=112+$i*305;$null=Rect $s $x 311 280 220 $pale
      $null=Text $s $steps[$i][0] ($x+20) 327 240 43 29 $dark $true
      $null=Text $s $steps[$i][1] ($x+20) 389 242 72 24 $ink $true
      $null=Text $s $steps[$i][2] ($x+20) 478 242 37 18 $muted
      if($i -lt 3){$null=Text $s '→' ($x+279) 396 26 43 23 $dark $true 2}
    }
    $null=Rect $s 113 576 1195 98 $dark
    $null=Text $s '다양한 전투 환경 → 결과 피드백 → 영구 성장 → 다음 전투' 135 606 1150 44 26 $white $false 2
    Note $s '이번 작업으로 콘텐츠 선택, 전투, 결과 확인, 영구 연구라는 반복 구조가 연결되었습니다. 스테이지별 환경의 차이와 포탑 외형 변화가 플레이 진행을 시각적으로 보여주고, 연구는 전투 이후 성장 요소를 담당합니다.'

    # 16 Closing.
    $s=$deck.Slides.Add($deck.Slides.Count+1,12);$s.DisplayMasterShapes=0;$s.FollowMasterBackground=0;$s.Background.Fill.ForeColor.RGB=0
    PixelBar $s 81 86 0;PixelBar $s 1140 86 172
    $null=Rect $s 91 244 1264 325 $dark;$null=Rect $s 81 234 1264 325 $white
    $null=Text $s '감사합니다' 130 320 1160 93 62 $ink $false 2
    $null=Text $s 'AI DEFENDERS  /  Q & A' 130 444 1160 42 27 $dark $false 2
    $null=Text $s '스테이지 확장 · 포탑 모델링 · 영구 연구 · 결과 화면' 130 637 1160 40 22 $white $false 2
    Note $s '발표를 마치고 질문을 받습니다.'

    $deck.SaveAs($output,24)
    $deck.SaveAs($pdf,32)
    $report=@()
    foreach($slide in $deck.Slides){
        $slide.Export((Join-Path $renders ('slide{0:00}.png' -f $slide.SlideIndex)),'PNG',1600,900)
        foreach($shape in $slide.Shapes){
            if($shape.HasTextFrame -eq -1 -and $shape.TextFrame.HasText -eq -1){
                $tr=$shape.TextFrame.TextRange
                $height2=$shape.TextFrame2.TextRange.BoundHeight
                if($height2 -gt ($shape.Height+8)){$report+=('Slide '+$slide.SlideIndex+' overflow: '+$tr.Text+' | height='+$height2+' frame='+$shape.Height)}
            }
        }
    }
    $report | Set-Content (Join-Path $build 'overflow.txt') -Encoding UTF8
    Write-Output ('Created '+$deck.Slides.Count+' slides: '+$output)
    Write-Output ('Overflow flags: '+$report.Count)
    $deck.Close()
} finally { $ppt.Quit() }
