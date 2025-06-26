param(
    [string]$Version = "1.0.1"
)

Write-Host "Building Uzman Raporu v$Version" -ForegroundColor Green

# Clean
if (Test-Path ".\release") {
    Remove-Item ".\release" -Recurse -Force
}

# Restore
Write-Host "Restoring packages..." -ForegroundColor Cyan
dotnet restore BilirkisiMasaustu.csproj
if ($LASTEXITCODE -ne 0) { exit 1 }

# Build
Write-Host "Building..." -ForegroundColor Cyan
dotnet build BilirkisiMasaustu.csproj --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

# Publish
Write-Host "Publishing..." -ForegroundColor Cyan
dotnet publish BilirkisiMasaustu.csproj --configuration Release --runtime win-x64 --self-contained true --output ".\release\publish"
if ($LASTEXITCODE -ne 0) { exit 1 }

# Check if exe exists
$exePath = ".\release\publish\BilirkisiMasaustu.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "ERROR: Executable not found at $exePath" -ForegroundColor Red
    Write-Host "Contents of publish folder:" -ForegroundColor Yellow
    Get-ChildItem ".\release\publish" -ErrorAction SilentlyContinue | Format-Table
    exit 1
}

# Create package
Write-Host "Creating package..." -ForegroundColor Cyan
$packageName = "UzmanRaporu-v$Version-win-x64"
$packageDir = ".\release\$packageName"

New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# Copy files
Copy-Item $exePath $packageDir
Copy-Item ".\iller" $packageDir -Recurse
if (Test-Path "README.md") { Copy-Item "README.md" $packageDir }
if (Test-Path "LICENSE") { Copy-Item "LICENSE" $packageDir }

# Create simple instructions
$instructions = @"
Uzman Raporu v$Version - Kurulum

1. BilirkisiMasaustu.exe dosyasina cift tiklayin
2. Uygulama dogrudan calisacaktir

Gereksinimler: Yok (.NET 8 dahil)
Gelistirici: Saffet Celik
E-posta: iletisim@saffetcelik.com
"@

Set-Content (Join-Path $packageDir "KURULUM.txt") $instructions

# Create ZIP
Write-Host "Creating ZIP..." -ForegroundColor Cyan
$zipPath = ".\release\$packageName.zip"
Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force

# Show results
$exeSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)

Write-Host ""
Write-Host "BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host "Executable: $exeSize MB" -ForegroundColor White
Write-Host "ZIP Package: $zipSize MB" -ForegroundColor White
Write-Host "Location: $zipPath" -ForegroundColor White
Write-Host ""
Write-Host "Ready for release!" -ForegroundColor Green
