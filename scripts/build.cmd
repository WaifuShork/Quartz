@echo off 

REM Clean (clean because of issues with builds often)
dotnet clean "..\src\Vivian.sln" --nologo

REM Build
dotnet build "..\src\Vivian.sln" --nologo || exit /b 

dotnet build "..\src\Vivian\Vivian.csproj" --nologo || exit /b 
dotnet build "..\src\Vivian.Compiler\Vivian.Compiler.csproj" --nologo || exit /b 
dotnet build "..\src\Vivian.Host\Vivian.Host.csproj" --nologo || exit /b 
dotnet build "..\src\Vivian.Sdk\Vivian.Sdk.csproj" --nologo || exit /b 

REM Test
dotnet test "..\src\Vivian.Tests" --nologo --no-build