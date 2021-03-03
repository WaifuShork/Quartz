@echo off

dotnet clean Playground.sln
dotnet build Playground.sln

dotnet run --project src\Playground.vivproj