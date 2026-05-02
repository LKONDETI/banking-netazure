---
name: run-tests
description: Run the .NET 9 test suite with coverage report
allowed-tools:
  - Bash(dotnet *)
---

Run the full test suite:

1. `dotnet test --configuration Release --logger "console;verbosity=normal"`
2. Report passing/failing tests
3. If failures exist, show the error message and failing test name
