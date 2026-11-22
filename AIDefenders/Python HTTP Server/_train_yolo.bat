@echo off
setlocal
set PYTHONUNBUFFERED=1
cd /d "C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server"
title YOLO train
echo Python 3.11 venv
echo C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe
echo Dataset: city
echo.
"C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -u "C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server\yolo_train.py" --data "C:/Users/user/Downloads/AIDefendersV5-main/AIDefenders/Assets\Datasets\city\dataset.yaml" --model "yolov8n.pt" --epochs 50 --batch 16 --workers 0 --dataset "city" --result "C:/Users/user/Downloads/AIDefendersV5-main/AIDefenders/Assets\Datasets\city\train_result.json" --weights-dir "C:/Users/user/Downloads/AIDefendersV5-main/AIDefenders/Assets\Datasets\city\weights" --stop-file "C:/Users/user/Downloads/AIDefendersV5-main/AIDefenders/Assets\Datasets\city\train_stop.txt"
if errorlevel 1 goto :fail
echo.
echo Training finished.
exit /b 0
:fail
echo.
echo Training failed.
pause
exit /b 1
