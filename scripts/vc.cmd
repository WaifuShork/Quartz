@echo off 

REM Clean (clean because of issues with builds often)
dotnet clean "..\src\Vivian.Compiler" --nologo

REM Build
dotnet build "..\src\Vivian.Compiler" --nologo || exit /b 

REM Run
dotnet run --project "..\src\Vivian.Compiler" --nologo || exit /b