#!/bin/bash

# Used to build Vivian for Linux

projectPath="..\src\Vivian.Compiler"
outputDir="..\artifacts"

mkdir outputDir

echo "Building compiler..."


dotnet restore "..\Vivian.sln"
dotnet publish $projectPath -o "$outputDir" -r linux-x64 -p:PublishReadyToRun=false -p:PublishSingleFile=true -p:PublishTrimmed=true -c Release --force --nologo --self-contained true -v normal