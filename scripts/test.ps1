# This script is used to run tests 

function Set-SolutionSetup
{
    Write-Output "Building, and Cleaning solution..."

    [string]$solutionDir = "..\Vivian.sln"
    dotnet.exe clean $solutionDir -c Release --nologo 
    dotnet.exe restore $solutionDir --force 
    dotnet.exe build $solutionDir -c Release --nologo --force

} Set-SolutionSetup

function Set-BuildProjects
{
    dotnet.exe build "..\src\Vivian\Vivian.csproj" --nologo
    dotnet.exe build "..\src\Vivian.Installer\Vivian.Installer.csproj" --nologo
    dotnet.exe build "..\src\Vivian.Tools\Vivian.Tools.csproj" --nologo
    dotnet.exe build "..\src\Vivian.Compiler\Vivian.Compiler.csproj" --nologo

} Set-BuildProjects

function Test-Vivian
{   
    Write-Output "Running tests..."
    # Reduce inlining hell lmao
    [string]$testProject = "..\src\Vivian.Tests"
    [string]$resultsPath = "..\src\Vivian.Tests\TestResults"

    # Force specific settings for the test env
    dotnet.exe test $testProject `
             --nologo -r $resultsPath `
              -v minimal -c Debug `
             --blame-hang-dump-type full `
             --blame-hang-timeout 30m `
             --no-build

} Test-Vivian