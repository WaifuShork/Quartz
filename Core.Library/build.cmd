@echo off

dotnet clean Core.sln
dotnet build Core.sln

dotnet run --project src\Core.vivproj