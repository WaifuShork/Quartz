# Clean (clean because of issues with builds often)
dotnet clean "..\src\Vivian.sln" --nologo

# Build
dotnet build "..\src\Vivian.Compiler" --nologo || exit

# Test
dotnet test "..\src\Vivian.Tests" --nologo --no-build