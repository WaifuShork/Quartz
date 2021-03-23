@echo off 

REM Clean (clean because of issues with builds often)
dotnet clean "src\Vivian.sln" --nologo

REM Build
dotnet build "src\Vivian.sln" --nologo || exit /b 

REM Test
dotnet test "src\Vivian.Tests" --nologo --no-build