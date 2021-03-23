@echo off

set outputdir=vivian\bin\Output\
set publishdir=vivian\bin\Release\net5.0\publish\
if not exist %outputdir% mkdir %outputdir%

set name="Portable (Framework Dependent)"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
tar -c -f %outputdir%vivian.Portable -C %publishdir%portable *

set name="Win x86"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
tar -c -f %outputdir%vivian.Win-x86 -C %publishdir%win-x86 *

set name="Win x64"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
tar -c -f %outputdir%vivian.Win-x64 -C %publishdir%win-x64 *

set name="Win ARM"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
tar -c -f %outputdir%vivian.Win-ARM -C %publishdir%win-arm *

set name="Linux x64"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
tar -c -f %outputdir%vivian.Linux-x64 -C %publishdir%linux-x64 *

set name="Linux ARM"
echo Publishing: %name%
dotnet publish -p:PublishProfile=%name%
tar -c -f %outputdir%vivian.Linux-ARM -C %publishdir%linux-arm *