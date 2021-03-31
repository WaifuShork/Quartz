@echo off

RMDIR /S /Q "artifacts\vivian"

set outputdir=vivian\bin\Output\
set publishdir=artifacts\vivian
if not exist %outputdir% mkdir %outputdir%

set name="Vivian Installer"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%

move "artifacts\vivian\Vivian.Installer.exe" "artifacts\vivian\vivian-installer.exe"
robocopy "vivian" "artifacts\vivian" /E
robocopy "..\..\artifacts\vivian" "artifacts\vivian\vivian"