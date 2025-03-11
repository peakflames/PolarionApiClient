
Write-Host "Using dotnet verison $(dotnet --version)"
Write-Host "Create NuGet packages in .\nuget-local directory..."

Write-Host "Clean entire solution..." -ForegroundColor Yellow
dotnet clean src/PolarionApiClient.sln
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Run tests..." -ForegroundColor Yellow
dotnet test src/PolarionApiClient.sln
if ($LASTEXITCODE -ne 0) { exit 1 }

# Build and pack Polarion package
Write-Host "Publish Polarion package..." -ForegroundColor Yellow
dotnet publish src/Polarion/Polarion.csproj -c Release
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "Pack Polarion package..." -ForegroundColor Yellow
dotnet pack src/Polarion/Polarion.csproj -c Release -o nuget-local
if ($LASTEXITCODE -ne 0) { exit 1 }

Write-Host "NuGet packages created in nuget-local directory" -ForegroundColor Green
