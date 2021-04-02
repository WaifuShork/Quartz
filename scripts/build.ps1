# Used to control Vivian build process for:
#
#   - restore
#   - build
#   - sign
#   - sign
#   - pack
#   - test
#   - publish

# <----------------->
# Test project
function Test-Vivian
{   
    Write-Output "Running tests..."
    # Reduce inlining hell lmao
    [string]$testProject = "..\src\Vivian.Tests"
    [string]$resultsPath = "..\artifacts\TestResults"

    # Force specific settings for the test env
    dotnet.exe test $testProject `
             --nologo -r $resultsPath `
              -v minimal -c Debug `
             --blame-hang-dump-type full `
             --blame-hang-timeout 30m `
             --no-build
    
    # Test before every build, if the tests are failing it makes no sense to create a release build 
    if (0 -ne $LASTEXITCODE)
    {
        Write-Output "Not all tests passed. Terminating build."
        exit
    }
} Test-Vivian

# <----------------->
# Clean, Restore, and Build solution file
function Set-SolutionBuildAndRestore
{
    [string]$solutionDir = "..\Vivian.sln"
    dotnet.exe clean $solutionDir -c Release --nologo 
    dotnet.exe restore $solutionDir --force 
    dotnet.exe build $solutionDir -c Release --nologo --force

} Set-SolutionBuildAndRestore

# <----------------->
# Clean previous builds & Setup structure
function Set-Structure
{
    Write-Output "Setting up build directories..."

    # Artifacts
    Remove-Item "..\artifacts" -Recurse -Force
    New-Item -Path "..\artifacts" -ItemType Directory

    # Installer
    New-Item "..\artifacts\installer\win-x64\vivian" -ItemType Directory
    
    # Apparently I'm a dumbass because I can't take the whole directory without it being fucked
    Copy-Item "..\resources\installer\win-x64\ref_modules" -Destination "..\artifacts\installer\win-x64\vivian" -Recurse
    Copy-Item "..\resources\installer\win-x64\templates" -Destination "..\artifacts\installer\win-x64\vivian" -Recurse
    Copy-Item "..\resources\installer\win-x64\logo.txt" -Destination "..\artifacts\installer\win-x64\vivian"

    # Compiler 
    # New-Item -Path '..\artifacts\compiler\vivian\win-x64' -ItemType Directory
    # Portable build of compiler still needs references for the templates
    Copy-Item "..\resources\installer\win-x64\ref_modules" -Destination "..\artifacts\compiler\vivian\win-x64\ref_modules" -Recurse
    Copy-Item "..\resources\installer\win-x64\templates" -Destination "..\artifacts\compiler\vivian\win-x64" -Recurse

} Set-Structure

# <----------------->
# x64 Windows Vivian Installer
function Set-Windows64Installer
{
    Write-Output "Building installer..."
    [string]$projectPath = "..\src\Vivian.Installer"
    [string]$outputDir = "..\artifacts\installer\win-x64"

    dotnet.exe publish $projectPath `
              -o $outputDir -r win-x64 `
              -p:PublishReadyToRun=true `
              -p:PublishSingleFile=true `
              -p:PublishTrimmed=true `
              -p:PublishReadyToRunShowWarnings=false `
              -c Release --force --nologo `
              --self-contained true `
              -v normal

    Rename-Item '..\artifacts\installer\win-x64\Vivian.Installer.exe' 'vivian-installer.exe'

} Set-Windows64Installer

# <----------------->
# x64 Windows Vivian Compiler
function Set-Vivian64Compiler
{
    Write-Output "Building compiler..."
    [string]$projectPath = "..\src\Vivian.Compiler"
    [string]$outputDir = "..\artifacts\compiler\vivian\win-x64"

    dotnet.exe publish $projectPath `
              -o $outputDir -r win-x64 `
              -p:PublishReadyToRun=true `
              -p:PublishSingleFile=true `
              -p:PublishTrimmed=true `
              -p:PublishReadyToRunShowWarnings=false `
              -c Release --force --nologo `
              --self-contained true `
              -v normal

    Rename-Item '..\artifacts\compiler\vivian\win-x64\Vivian.Compiler.exe' 'vivian.exe'

} Set-Vivian64Compiler

function Set-AddCompilerToInstallation
{
    # Lets try this again
    Copy-Item "..\artifacts\compiler\vivian\win-x64\clrcompression.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
    Copy-Item "..\artifacts\compiler\vivian\win-x64\clrjit.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
    Copy-Item "..\artifacts\compiler\vivian\win-x64\coreclr.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
    Copy-Item "..\artifacts\compiler\vivian\win-x64\mscordaccore.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
    Copy-Item "..\artifacts\compiler\vivian\win-x64\vivian.exe" -Destination "..\artifacts\installer\win-x64\vivian\"

} Set-AddCompilerToInstallation