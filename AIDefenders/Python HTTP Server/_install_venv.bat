@echo off
setlocal
chcp 65001 >nul
cd /d "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server"
title YOLO library setup
echo Python 3.11 virtualenv / library setup
echo GPU: NVIDIA GeForce RTX 5060
echo Torch: cu130
echo.
if exist "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" goto :pip
echo Creating virtualenv...
where py >nul 2>nul
if not errorlevel 1 (
  py -3.11 -m venv "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv"
) else (
  py -3.11 -m venv "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv"
)
if errorlevel 1 goto :fail
:pip
echo Upgrading pip...
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -m pip install --upgrade pip
if errorlevel 1 goto :fail
echo Removing previous torch if present...
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -m pip uninstall -y torch torchvision torchaudio
echo Installing CUDA PyTorch (cu130)...
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -m pip install torch torchvision --index-url https://download.pytorch.org/whl/cu130
if errorlevel 1 (
  echo CUDA torch install failed. Falling back to CPU torch.
  "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -m pip install torch torchvision --index-url https://download.pytorch.org/whl/cpu
  if errorlevel 1 goto :fail
)
echo Installing requirements...
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -m pip install -r "C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\requirements.txt"
if errorlevel 1 goto :fail
echo.
echo Checking torch.cuda...
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -c "import torch; print('torch', torch.__version__); print('cuda', torch.cuda.is_available()); print('device', torch.cuda.get_device_name(0) if torch.cuda.is_available() else 'cpu')"
echo.
"C:\Users\user\Documents\캡스톤 프로젝트\AIDefendersV5\AIDefenders\Python HTTP Server\.venv\Scripts\python.exe" -c "import torch; device = 'cuda' if torch.cuda.is_available() else 'cpu'; x = torch.ones((16, 16), device=device); print('Kernel check:', (x @ x).sum().item())"
if errorlevel 1 goto :fail
echo Setup finished.
pause
exit /b 0
:fail
echo.
echo Setup failed.
pause
exit /b 1
