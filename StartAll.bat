@echo off
echo Starting API...
start "S2O1 API" dotnet run --project "%~dp0S2O1.API" --urls "http://localhost:5267"

echo Starting CLI...
start "S2O1 CLI" dotnet run --project "%~dp0S2O1.CLI"

echo Done. API is running at http://localhost:5267
pause
