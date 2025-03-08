# Get NuGet API Key from environment variable
$apiKey = $env:NUGET_API_KEY

# Error handling function
function Write-ErrorAndExit {
  param(
    $message
  )
  Write-Host $message -ForegroundColor Red
  exit 1
}

# Check for NuGet API Key in environment variable
if (-not $apiKey) {
  Write-ErrorAndExit "NuGet API Key not found in environment variable 'NUGET_API_KEY'. Please set the environment variable and try again."
}

# Check if current branch is main
$currentBranch = git rev-parse --abbrev-ref HEAD
if ($currentBranch -ne "main") {
  Write-ErrorAndExit "Publishing is only allowed from the 'main' branch. Current branch: $currentBranch"
}

# Clear all the old nuget-local folder
Write-Host "Clear all the old nuget-local folder..."
rm -r -force .\nuget-local

# Build and pack Polarion package
Write-Host "Building and packing Polarion package..."
# Build and pack Polarion package
dotnet clean src/Polarion/Polarion.csproj
if ($LASTEXITCODE -ne 0) { 
  Write-ErrorAndExit "Error occurred while cleaning Polarion package." 
}

dotnet publish src/Polarion/Polarion.csproj -c Release
if ($LASTEXITCODE -ne 0) { 
  Write-ErrorAndExit "Error occurred while preparing Polarion package." 
}

dotnet pack src/Polarion/Polarion.csproj -c Release -o nuget-local
if ($LASTEXITCODE -ne 0) { 
  Write-ErrorAndExit "Error occurred while packing Polarion package." 
}

Write-Host "NuGet packages created in nuget-local directory" -ForegroundColor Green

# Publish to NuGet.org
Write-Host "Publishing Polarion library to NuGet.org..."

# Publish Polarion package
dotnet nuget push .\nuget-local\Polarion.*.nupkg -s https://api.nuget.org/v3/index.json -k $apiKey --skip-duplicate
if ($LASTEXITCODE -ne 0) {
  Write-ErrorAndExit "An error occurred while publishing Polarion package."
}

Write-Host "Polarion library published successfully to NuGet.org!" -ForegroundColor Green
Write-Host ""
Write-Host "Goto URL https://www.nuget.org/packages?q=polarion to view the published packages."
Write-Host ""