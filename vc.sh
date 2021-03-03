#!/bin/bash

# Vars
slndir="$(dirname "${BASH_SOURCE[0]}")/src"

# Restore + Build
dotnet build "$slndir/Vivian.Compiler" --nologo || exit

# Run
dotnet run -p "$slndir/Vivian.Compiler" --no-build -- "$@"