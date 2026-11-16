from pathlib import Path
p=Path('Assets/Scripts/CampaignSelectionUi.cs');s=p.read_text(encoding='utf-8-sig')
s=s.replace('this.stages = AnyangCampusStage.Include(stages);','this.stages = Array.FindAll(stages ?? Array.Empty<CampaignStageEntry>(), entry => entry != null && entry.chapterId != 99 && entry.sceneName != "AnyangCampus_1");')
s=s.replace('stage.chapterId != AnyangCampusStage.ChapterId && ','')
s=s.replace('        UiSkin.Button(content, "HiddenAnyang", "히든 · 안양대학교  ›", UiSkin.Blue,\n            new Vector2(.5f, 0), new Vector2(0, -34), new Vector2(340, 44))\n            .onClick.AddListener(() => ShowStages(AnyangCampusStage.ChapterId));\n','')
s=s.replace('(id == AnyangCampusStage.ChapterId ? "HIDDEN" : "CHAPTER " + id.ToString("00"))','("CHAPTER " + id.ToString("00"))')
s=s.replace('id == AnyangCampusStage.ChapterId ? entries.Count : Mathf.Max(3, entries.Count)','Mathf.Max(3, entries.Count)')
s=s.replace('id == AnyangCampusStage.ChapterId || ','')
s=s.replace('id == AnyangCampusStage.ChapterId ? "캠퍼스를 지켜라" : ','')
s=s.replace('id == AnyangCampusStage.ChapterId ? "경사진 진입로와 교정, 언덕 위의 특별한 방어전" : ','')
assert 'AnyangCampusStage' not in s and 'HiddenAnyang' not in s
p.write_text(s,encoding='utf-8')
p=Path('Assets/Scripts/SceneLoader.cs');s=p.read_text(encoding='utf-8-sig');s='\n'.join(l for l in s.split('\n') if 'name.Contains("anyangcampus")' not in l);p.write_text(s,encoding='utf-8')
p=Path('ProjectSettings/EditorBuildSettings.asset');s=p.read_text(encoding='utf-8-sig');s=s.replace('  - enabled: 1\n    path: Assets/Scenes/GameStages/AnyangCampus/AnyangCampus_1.unity\n    guid: 3b8637fa4a976f340834c79817cde9c5\n','');assert 'AnyangCampus' not in s;p.write_text(s,encoding='utf-8')
