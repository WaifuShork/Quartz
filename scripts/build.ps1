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
} 

# <----------------->
# Clean, Restore, and Build solution file
function Set-SolutionRestore
{
    [string]$solutionDir = "..\Vivian.sln"
    dotnet.exe restore $solutionDir --force 
    # dotnet.exe build $solutionDir -c Release --nologo --force
} 

# <----------------->
# Clean previous builds & Setup structure
function Set-Structure
{
    Write-Output "Setting up build directories..."

    # Artifacts
    Remove-Item "..\artifacts" -Recurse -Force
    New-Item -Path "..\artifacts" -ItemType Directory

    # Installer
    New-Item "..\artifacts\installer\win-x64" -ItemType Directory

    [string]$file = "..\resources\vivian-x64.zip"
    if (Test-Path -Path $file  -PathType Leaf) 
    {
        try 
        {
            $null = Remove-Item $file -Force
            Write-Host "Removed Vivian-x64.zip"
        }
        catch 
        {
            throw $_.Exception.Message
        }
    }
    else 
    {
        Write-Host $file + " did not exist, generating with build"    
    }

    [string]$compilerFile = "..\resources\temp\Vivian.Compiler.exe"
    if (Test-Path -Path $compilerFile -PathType Leaf) 
    {
        try 
        {
            $null = Remove-Item $compilerFile -Force
        }
        catch 
        {
            throw $_.Exception.Message
        }
    }

    if (Test-Path -Path "..\resources\temp\vivian.exe" -PathType Leaf) 
    {
        try 
        {
            $null = Remove-Item "..\resources\temp\vivian.exe" -Force
        }
        catch 
        {
            throw $_.Exception.Message
        }
    }
} 

# <----------------->
# x64 Windows Vivian Compiler
function Set-Vivian64Compiler
{
    Write-Output "Building compiler..."
    [string]$projectPath = "..\src\Vivian.Compiler"
    [string]$outputDir = "..\resources\temp\"

    dotnet.exe publish $projectPath `
                       -o $outputDir -r win-x64 `
                       -p:PublishReadyToRun=true `
                       -p:PublishSingleFile=true `
                       -p:PublishTrimmed=true `
                       -p:PublishReadyToRunShowWarnings=false `
                       -p:IncludeNativeLibrariesForSelfExtract=true `
                       -c Release  `
                       --self-contained true `
                       -v normal `
                       --force --nologo
}

# <----------------->
# Generate installer resources
function Set-GenerateResources
{
    Rename-Item -Path "..\resources\temp\Vivian.Compiler.exe" -NewName "vivian.exe"

    Compress-Archive -LiteralPath "..\resources\temp\ref_modules", `
                                  "..\resources\temp\templates", `
                                  "..\resources\temp\logo.txt", `
                                  "..\resources\temp\vivian.exe" `
                                   -DestinationPath "..\resources\vivian-x64.zip"
}

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
              -p:IncludeNativeLibrariesForSelfExtract=true `
              -c Release --force --nologo `
              --self-contained true `
              -v normal

    Rename-Item "..\artifacts\installer\win-x64\Vivian.Installer.exe" -NewName "vivian-installer.exe"
} 

Test-Vivian
Set-SolutionRestore
Set-Structure
Set-Vivian64Compiler
Set-GenerateResources
Set-Windows64Installer

# function Set-AddCompilerToInstallation
# {
#     Copy-Item "..\artifacts\compiler\vivian\win-x64\clrcompression.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x64\clrjit.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x64\coreclr.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x64\mscordaccore.dll" -Destination "..\artifacts\installer\win-x64\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x64\vivian.exe" -Destination "..\artifacts\installer\win-x64\vivian\"
# 
#     Copy-Item "..\artifacts\compiler\vivian\win-x86\clrcompression.dll" -Destination "..\artifacts\installer\win-x86\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x86\clrjit.dll" -Destination "..\artifacts\installer\win-x86\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x86\coreclr.dll" -Destination "..\artifacts\installer\win-x86\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x86\mscordaccore.dll" -Destination "..\artifacts\installer\win-x86\vivian\"
#     Copy-Item "..\artifacts\compiler\vivian\win-x86\vivian.exe" -Destination "..\artifacts\installer\win-x86\vivian\"
# 
# } Set-AddCompilerToInstallation

# function Set-ZippedFolders
# {
#     Compress-Archive -LiteralPath "..\artifacts\compiler\vivian\win-x64" -DestinationPath "..\artifacts\compiler\vivian\win-x64"
#     Compress-Archive -LiteralPath "..\artifacts\compiler\vivian\win-x86" -DestinationPath "..\artifacts\compiler\vivian\win-x86"
#     Compress-Archive -LiteralPath "..\artifacts\installer\win-x64" -DestinationPath "..\artifacts\installer\win-x64"
#     Compress-Archive -LiteralPath "..\artifacts\installer\win-x86" -DestinationPath "..\artifacts\installer\win-x86"
# 
# } Set-ZippedFolders