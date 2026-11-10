@echo off
setlocal
chcp 65001 >nul
set PYTHONUNBUFFERED=1
cd /d "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server"
title YOLO train
echo Python 3.11 venv
echo C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe
echo Dataset: 123
echo.
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -u "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\yolo_train.py" --data "C:/Users/user/Documents/캡스톤 프로젝트/AIDefendersV5/AIDefenders/Assets\Datasets\123\dataset.yaml" --epochs 7 --batch 1 --workers 0 --dataset "123" --result "C:/Users/user/Documents/캡스톤 프로젝트/AIDefendersV5/AIDefenders/Assets\Datasets\123\train_result.json" --weights-dir "C:/Users/user/Documents/캡스톤 프로젝트/AIDefendersV5/AIDefenders/Assets\Datasets\123\weights"
if errorlevel 1 goto :fail
echo.
echo Training finished.
exit /b 0
:fail
echo.
echo Training failed.
pause
exit /b 1
