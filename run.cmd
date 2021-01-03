@echo off

REM Vars
set "SLNDIR=%~dp0src"

dotnet run --project "%SLNDIR%\WSC\WSC.csproj" --nologo