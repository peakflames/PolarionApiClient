
Write-Host "Using dotnet verison $(dotnet --version)"
Write-Host "Create NuGet packages in .\nuget-local directory..."

# Build and pack Polarion package
dotnet clean src/Polarion/Polarion.csproj
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet publish src/Polarion/Polarion.csproj -c Release
if ($LASTEXITCODE -ne 0) { exit 1 }
dotnet pack src/Polarion/Polarion.csproj -c Release -o nuget-local
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "NuGet packages created in nuget-local directory" -ForegroundColor Green
