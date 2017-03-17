@echo off

setlocal

set INSTALL_CMD=C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe
set SERVICE_NAME=VRAS
set OPTION=/i

if /i "%1" == "/u" (
  set OPTION=/u
  shift /1
)

if not "%1" == "" (
  set SERVICE_NAME=%1
  shift /1
)

%INSTALL_CMD% /ServiceName=%SERVICE_NAME% %OPTION% VRAS.exe

exit /b 0