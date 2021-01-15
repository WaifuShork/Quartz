@echo off

REM Vars
set "SLNDIR=%~dp0src"

dotnet run --project "%SLNDIR%\Vivian.Compiler\Vivian.Compiler.csproj" -- %*