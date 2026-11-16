from pathlib import Path
import json, math
import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
W,H,FPS = 1920,1080,24
FONT = 'C:/Windows/Fonts/malgun.ttf'
BOLD = 'C:/Windows/Fonts/malgunbd.ttf'
def font(size,bold=False): return ImageFont.truetype(BOLD if bold else FONT,size)
# Highlight rectangles are normalized to the original game screenshot.
shots = [
 ('01_MainMenu.png','AI 디펜더스 · UI 사용 가이드','데이터를 준비하고, 포탑을 연구하고, 기지를 방어하세요.',5,[]),
 ('01_MainMenu.png','메인 메뉴','캠페인 시작은 전투로, 포탑 연구소는 영구 강화로 이어집니다.\n데이터셋 팩토리에서는 AI 학습용 데이터를 준비합니다.',7,[(.33,.64,.68,.76),(.21,.77,.69,.9)]),
 ('02_Settings.png','설정','전체 음량·배경음·효과 및 환경음을 각각 조절합니다.\n카메라 탭에서는 카메라 조작 감도를 설정할 수 있습니다.',7,[(.36,.34,.64,.75)]),
 ('03_Chapters-1.png','챕터 선택','초원·사막·도시 중 원하는 챕터를 선택하세요.\n챕터를 선택하면 해당 지역의 스테이지가 열립니다.',7,[(.05,.24,.95,.84)]),
 ('04_GrassStages.png','스테이지 선택','초원은 낮·저녁·밤으로 구성되어 있습니다.\n스테이지 정보와 등장 대상을 확인한 뒤 전투를 시작합니다.',8,[(.14,.55,.54,.75),(.62,.76,.92,.86)]),
 ('05_TurretLab.png','포탑 연구소 · 영구 업그레이드','오른쪽 목록에서 포탑을 고르면 성능과 연구 항목이 표시됩니다.\n연구 포인트로 해금한 영구 강화는 이후 전투에 적용됩니다.',8,[(.73,.02,.99,.31),(.73,.46,.99,.92)]),
 ('06_TurretPreview.png','포탑 단계 미리보기','1·2·3단계 버튼으로 단계별 모델과 성능을 살펴보세요.\n미리보기 선택과 연구 포인트를 쓰는 영구 연구는 별도입니다.',7,[(.74,.52,.99,.61)]),
 ('07_Dataset.png','데이터셋 팩토리','왼쪽에서 데이터셋을 선택하거나 새 데이터셋을 만듭니다.\n편집 버튼을 누르면 샘플과 촬영 조건을 확인할 수 있습니다.',7,[(.01,.12,.23,.3),(.24,.39,.58,.49)]),
 ('08_DatasetEditor.png','샘플 생성 설정','촬영 대상·지형·대상 수와 카메라 조건을 설정합니다.\n샘플 목록과 미리보기를 확인하고 학습용 이미지를 준비하세요.',8,[(.01,.12,.23,.85),(.75,.12,.99,.94)]),
 ('09_Training.png','AI 모델 학습','학습 창에서 에포크와 배치 크기를 정합니다.\n학습이 끝난 모델은 전투 시작 시 모델 선택에 사용됩니다.',7,[(.36,.36,.64,.61)]),
 ('10_Battle.png','전투 화면 · 상태 확인','상단의 하트는 기지 체력, 코인은 사용 가능한 자금입니다.\n라운드와 남은 준비 시간을 보며 방어를 준비하세요.',8,[(.01,.01,.24,.11),(.77,.01,.95,.11)]),
 ('11_Build.png','건설 메뉴','오른쪽 건설 메뉴에서 플랫폼과 포탑을 선택합니다.\n보유 자금과 설치 위치를 확인해 방어선을 구성하세요.',8,[(.77,.15,.99,.93)]),
 ('12_Pause.png','일시 정지','오른쪽 위 일시정지 버튼 또는 Esc로 전투를 멈춥니다.\n음량 조절·계속하기·다시 시작·메인 메뉴 이동이 가능합니다.',7,[(.29,.2,.72,.77)]),
 ('../UIRefresh/Result.png','결과 화면 예시','전투 종료 후 점수·처치 수·피아식별 정확도를 확인합니다.\n연구 포인트 획득 내역을 보고 다음 연구를 준비하세요.',7,[(.3,.15,.7,.86)]),
 ('01_MainMenu.png','다음 전투를 준비하세요','데이터셋 제작 → AI 학습 → 기지 방어 → 포탑 연구\n메인 메뉴에서 원하는 기능을 선택해 시작하세요.',5,[]),
]

def source(name):
 p=ROOT/'Captures'/name
 return p if p.exists() else ROOT/name

def stamp(seconds):
 ms=round(seconds*1000);return f'{ms//3600000:02}:{ms//60000%60:02}:{ms//1000%60:02},{ms%1000:03}'

def make_slide(i,name,title,caption):
 canvas=Image.new('RGB',(W,H),'#091c30');d=ImageDraw.Draw(canvas)
 d.rectangle((0,0,W,79),fill='#103555')
 d.text((45,18),'AI DEFENDERS',font=font(29,True),fill='#6cdbf5')
 d.text((380,15),title,font=font(34,True),fill='white')
 d.text((1710,22),f'{i+1:02} / {len(shots):02}',font=font(25,True),fill='#a3c7df')
 im=Image.open(source(name)).convert('RGB')
 im.thumbnail((1780,824),Image.Resampling.LANCZOS)
 x=(W-im.width)//2;y=85+(824-im.height)//2
 canvas.paste(im,(x,y))
 d=ImageDraw.Draw(canvas)
 d.rectangle((0,920,W,H),fill='#0b263f')
 d.rectangle((45,946,53,1039),fill='#ffd044')
 lines=caption.split('\n')
 for j,line in enumerate(lines):
  d.text((80,943+j*51),line,font=font(35,True),fill='white')
 return cv2.cvtColor(np.array(canvas),cv2.COLOR_RGB2BGR),(x,y,im.width,im.height)

def main():
 path=ROOT/'AI_Defenders_UI_Guide.mp4'
 writer=cv2.VideoWriter(str(path),cv2.VideoWriter_fourcc(*'mp4v'),FPS,(W,H))
 if not writer.isOpened(): raise RuntimeError('Video encoder could not open')
 elapsed=0;srt=[];previews=[];timeline=[]
 total=sum(s[3] for s in shots)
 for i,(name,title,caption,duration,rects) in enumerate(shots):
  base,(x,y,iw,ih)=make_slide(i,name,title,caption)
  srt.append(f'{i+1}\n{stamp(elapsed)} --> {stamp(elapsed+duration)}\n{title}\n{caption}\n')
  timeline.append(dict(start=elapsed,duration=duration,title=title,caption=caption,image=name))
  frames=duration*FPS
  for n in range(frames):
   t=n/FPS;frame=base.copy()
   if rects and t>.7:
    # Sequential spotlights guide the viewer without hiding the UI.
    k=min(len(rects)-1,int((t-.7)/max(1,(duration-1)/len(rects))))
    a,b,c,e=rects[k];p=(round(x+a*iw),round(y+b*ih));q=(round(x+c*iw),round(y+e*ih))
    cv2.rectangle(frame,p,q,(55,210,255),4,cv2.LINE_AA)
    r=round(10+3*math.sin(t*4))
    cv2.circle(frame,(q[0]-10,q[1]-10),r,(55,210,255),3,cv2.LINE_AA)
   # Thin global progress bar and gentle fades between screens.
   cv2.rectangle(frame,(0,H-6),(round(W*(elapsed+t)/total),H),(55,210,255),-1)
   fade=min(1,t/.3,(duration-t)/.3)
   if fade<1: frame=cv2.convertScaleAbs(frame,alpha=max(0,fade))
   writer.write(frame)
   if n==2*FPS:
    thumb=cv2.resize(frame,(640,360));previews.append(thumb)
  elapsed+=duration
  print(f'{i+1}/{len(shots)} {title}',flush=True)
 writer.release()
 (ROOT/'AI_Defenders_UI_Guide.srt').write_text('\n'.join(srt),encoding='utf-8-sig')
 (ROOT/'timeline.json').write_text(json.dumps(timeline,ensure_ascii=False,indent=2),encoding='utf-8')
 sheet=np.zeros((360*5,640*3,3),np.uint8)
 for i,p in enumerate(previews):sheet[(i//3)*360:(i//3+1)*360,(i%3)*640:(i%3+1)*640]=p
 Image.fromarray(cv2.cvtColor(sheet,cv2.COLOR_BGR2RGB)).save(ROOT/'Preview.jpg')
 cap=cv2.VideoCapture(str(path))
 info=dict(width=int(cap.get(3)),height=int(cap.get(4)),fps=cap.get(5),frames=int(cap.get(7)),seconds=elapsed,bytes=path.stat().st_size)
 assert cap.isOpened() and info['frames']==elapsed*FPS,info
 for sec in [2,30,60,90,elapsed-2]:
  cap.set(cv2.CAP_PROP_POS_MSEC,sec*1000);ok,f=cap.read();assert ok and f.mean()>10
 cap.release();(ROOT/'validation.json').write_text(json.dumps(info,indent=2),encoding='utf-8')
 print(info,flush=True)

if __name__=='__main__':main()
