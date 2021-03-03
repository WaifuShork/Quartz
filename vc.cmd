@echo off

REM Vars
set "SLNDIR=%~dp0src"

REM Restore + Builde
dotnet build "%SLNDIR%\Vivian.Compiler" --nologo || exit /b

REM Run 
dotnet run -p  "%SLNDIR%\Vivian.Compiler" --no-build -- %*