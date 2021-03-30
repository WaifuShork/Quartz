@echo off

RMDIR /S /Q "..\..\artifacts\vivian"

set outputdir=vivian\bin\Output\
set publishdir=..\..\artifacts\vivian
if not exist %outputdir% mkdir %outputdir%

set name="vivian"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%

move "..\..\artifacts\vivian\Vivian.Compiler.exe" "..\..\artifacts\vivian\vivian.exe"