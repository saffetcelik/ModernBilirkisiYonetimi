# Basit Release Build Script
param(
    [string]$Version = "1.0.1"
)

Write-Host "🚀 Uzman Raporu Release Build" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Green

# Temizlik
if (Test-Path ".\release") {
    Remove-Item ".\release" -Recurse -Force
    Write-Host "Eski build temizlendi" -ForegroundColor Yellow
}

# Restore
Write-Host "📦 Dependencies restore..." -ForegroundColor Cyan
dotnet restore BilirkisiMasaustu.csproj
if ($LASTEXITCODE -ne 0) { exit 1 }

# Build
Write-Host "🔨 Building..." -ForegroundColor Cyan
dotnet build BilirkisiMasaustu.csproj --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

# Publish
Write-Host "📦 Publishing..." -ForegroundColor Cyan
dotnet publish BilirkisiMasaustu.csproj --configuration Release --runtime win-x64 --self-contained true --output .\release\publish
if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish başarısız!" -ForegroundColor Red
    exit 1
}

# Publish sonucunu kontrol et
if (-not (Test-Path ".\release\publish\BilirkisiMasaustu.exe")) {
    Write-Host "HATA: Publish sonrası executable bulunamadı!" -ForegroundColor Red
    Write-Host "Publish klasörü içeriği:" -ForegroundColor Yellow
    Get-ChildItem ".\release\publish" -ErrorAction SilentlyContinue
    exit 1
}

# Package oluştur
Write-Host "📁 Creating package..." -ForegroundColor Cyan
$packageName = "UzmanRaporu-v$Version-win-x64"
$packageDir = ".\release\$packageName"

New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# Dosyaları kopyala
if (Test-Path ".\release\publish\BilirkisiMasaustu.exe") {
    Copy-Item ".\release\publish\BilirkisiMasaustu.exe" $packageDir
    Write-Host "Executable kopyalandı" -ForegroundColor Green
} else {
    Write-Host "HATA: Executable bulunamadı!" -ForegroundColor Red
    exit 1
}

Copy-Item ".\iller" $packageDir -Recurse
if (Test-Path "README.md") { Copy-Item "README.md" $packageDir }
if (Test-Path "LICENSE") { Copy-Item "LICENSE" $packageDir }

# ZIP oluştur
Compress-Archive -Path "$packageDir\*" -DestinationPath ".\release\$packageName.zip" -Force

# Boyutları göster
$exeSize = [math]::Round((Get-Item ".\release\publish\BilirkisiMasaustu.exe").Length / 1MB, 2)
$zipSize = [math]::Round((Get-Item ".\release\$packageName.zip").Length / 1MB, 2)

Write-Host "✅ Build tamamlandı!" -ForegroundColor Green
Write-Host "Executable: $exeSize MB" -ForegroundColor White
Write-Host "ZIP: $zipSize MB" -ForegroundColor White
Write-Host "Konum: .\release\$packageName.zip" -ForegroundColor White
