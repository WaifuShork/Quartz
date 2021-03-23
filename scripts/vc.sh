# Clean (clean because of issues with builds often)
dotnet clean "..\src\Vivian.Compiler" --nologo

# Build
dotnet build "..\src\Vivian.Compiler" --nologo || exit

# Run
dotnet run --project "..\src\Vivian.Compiler" --no-build -- "$@"