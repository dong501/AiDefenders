@echo off
setlocal
set PYTHONUNBUFFERED=1
cd /d "C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server"
title YOLO server
echo Python 3.11 venv
echo Weights: C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Assets\Datasets\grass1\weights\best.pt
echo Health: http://127.0.0.1:8000/health
echo.
"C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -u "C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Python HTTP Server\server.py" --weights "C:\Users\user\Downloads\AIDefendersV5-main\AIDefenders\Assets\Datasets\grass1\weights\best.pt" --host 127.0.0.1 --port 8000
if errorlevel 1 pause
exit /b %ERRORLEVEL%
