@echo off
setlocal
chcp 65001 >nul
set PYTHONUNBUFFERED=1
cd /d "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server"
title YOLO server
echo Python 3.11 venv
echo Weights: C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Assets\Datasets\123\weights\best.pt
echo Health: http://127.0.0.1:8000/health
echo.
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -u "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\server.py" --weights "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Assets\Datasets\123\weights\best.pt" --host 127.0.0.1 --port 8000
if errorlevel 1 pause
exit /b %ERRORLEVEL%
