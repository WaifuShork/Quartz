@echo off

REM Vars
set "SLNDIR=%~dp0src"

REM Restore + Build
dotnet build "%SLNDIR%\Vivian.sln" --nologo || exit /b

REM Test
dotnet test "%SLNDIR%\Vivian.Tests" --nologo --no-build