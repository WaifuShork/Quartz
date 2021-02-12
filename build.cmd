@echo off

dotnet clean src\Vivian.sln
dotnet build src\Vivian.sln
dotnet test src\Vivian.sln