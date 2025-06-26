# Uzman Raporu - Performanslı Release Build Script
# Bu script, uygulamanın optimize edilmiş release sürümünü oluşturur

param(
    [string]$Version = "1.0.0",
    [string]$OutputDir = ".\release",
    [switch]$Clean = $false,
    [switch]$SkipTests = $false
)

# Renkli çıktı için fonksiyonlar
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Script başlangıcı
Write-Info "🚀 Uzman Raporu Release Build Script"
Write-Info "Version: $Version"
Write-Info "Output Directory: $OutputDir"
Write-Info "=" * 50

# Temizlik işlemi
if ($Clean) {
    Write-Info "🧹 Temizlik yapılıyor..."
    if (Test-Path $OutputDir) {
        Remove-Item $OutputDir -Recurse -Force
        Write-Success "Eski build dosyaları temizlendi"
    }
    
    if (Test-Path ".\bin") {
        Remove-Item ".\bin" -Recurse -Force
    }
    
    if (Test-Path ".\obj") {
        Remove-Item ".\obj" -Recurse -Force
    }
}

# Çıktı dizinini oluştur
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# .NET SDK kontrolü
Write-Info "🔍 .NET SDK kontrolü..."
try {
    $dotnetVersion = dotnet --version
    Write-Success ".NET SDK bulundu: $dotnetVersion"
} catch {
    Write-Error ".NET 8 SDK bulunamadı! Lütfen .NET 8 SDK'yı yükleyin."
    exit 1
}

# Proje dosyasında version güncelleme
Write-Info "📝 Version güncelleniyor..."
$csprojPath = "BilirkisiMasaustu.csproj"
if (Test-Path $csprojPath) {
    $content = Get-Content $csprojPath -Raw
    $assemblyVersionPattern = '<AssemblyVersion>.*?</AssemblyVersion>'
    $fileVersionPattern = '<FileVersion>.*?</FileVersion>'
    $versionPattern = '<Version>.*?</Version>'

    $content = $content -replace $assemblyVersionPattern, "<AssemblyVersion>$Version.0</AssemblyVersion>"
    $content = $content -replace $fileVersionPattern, "<FileVersion>$Version.0</FileVersion>"
    $content = $content -replace $versionPattern, "<Version>$Version</Version>"

    Set-Content $csprojPath $content -NoNewline
    Write-Success "Version $Version olarak güncellendi"
} else {
    Write-Error "BilirkisiMasaustu.csproj dosyası bulunamadı!"
    exit 1
}

# Dependencies restore
Write-Info "📦 Dependencies restore ediliyor..."
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Dependencies restore başarısız!"
    exit 1
}
Write-Success "Dependencies başarıyla restore edildi"

# Build
Write-Info "🔨 Solution build ediliyor..."
dotnet build --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build başarısız!"
    exit 1
}
Write-Success "Build başarıyla tamamlandı"

# Tests (opsiyonel)
if (-not $SkipTests) {
    Write-Info "🧪 Testler çalıştırılıyor..."
    dotnet test --configuration Release --no-build --verbosity normal
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Bazı testler başarısız oldu, ancak build devam ediyor..."
    } else {
        Write-Success "Tüm testler başarıyla geçti"
    }
}

# Publish - Performanslı single-file executable
Write-Info "📦 Performanslı executable oluşturuluyor..."
$publishDir = Join-Path $OutputDir "publish"

dotnet publish BilirkisiMasaustu.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $publishDir `
    /p:PublishSingleFile=true `
    /p:EnableCompressionInSingleFile=true `
    /p:DebuggerSupport=false `
    /p:PublishReadyToRun=true `
    /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish işlemi başarısız!"
    exit 1
}

# Release paketi oluşturma
Write-Info "📁 Release paketi hazırlanıyor..."
$packageName = "UzmanRaporu-v$Version-win-x64"
$packageDir = Join-Path $OutputDir $packageName

# Paket dizinini oluştur
New-Item -ItemType Directory -Path $packageDir -Force | Out-Null

# Exe dosyasını kopyala
$exePath = Join-Path $publishDir "BilirkisiMasaustu.exe"
if (Test-Path $exePath) {
    Copy-Item $exePath $packageDir
    Write-Success "Executable kopyalandı"
} else {
    Write-Error "Executable dosyası bulunamadı: $exePath"
    exit 1
}

# İller klasörünü kopyala
if (Test-Path ".\iller") {
    Copy-Item ".\iller" $packageDir -Recurse
    Write-Success "İller klasörü kopyalandı"
} else {
    Write-Warning "İller klasörü bulunamadı"
}

# README ve LICENSE dosyalarını kopyala
if (Test-Path "README.md") {
    Copy-Item "README.md" $packageDir
    Write-Success "README.md kopyalandı"
}

if (Test-Path "LICENSE") {
    Copy-Item "LICENSE" $packageDir
    Write-Success "LICENSE kopyalandı"
}

# Kurulum talimatları dosyası oluştur
$installInstructions = "# Uzman Raporu v$Version - Kurulum Talimatlari`n`n" +
"## Hizli Baslangic`n" +
"1. Bu klasordeki BilirkisiMasaustu.exe dosyasina cift tiklayin`n" +
"2. Uygulama dogrudan calisacaktir - ek kurulum gerekmez!`n`n" +
"## Ozellikler`n" +
"- Self-contained: .NET 8 runtime dahil`n" +
"- Tek dosya: Tum bagimliliklar dahil`n" +
"- Optimize edilmis: Hizli baslatma ve dusuk bellek kullanimi`n" +
"- Tasinabilir: Herhangi bir Windows bilgisayarda calisir`n`n" +
"## Sistem Gereksinimleri`n" +
"- Windows 10/11 (x64)`n" +
"- Minimum 100 MB bos disk alani`n" +
"- Ek yazilim gerektirmez`n`n" +
"## Iletisim`n" +
"- Gelistirici: Saffet Celik`n" +
"- E-posta: iletisim@saffetcelik.com`n" +
"- GitHub: github.com/saffetcelik`n`n" +
"## Lisans`n" +
"Bu yazilim Apache 2.0 lisansi altinda dagitilmaktadir."

Set-Content (Join-Path $packageDir "KURULUM.txt") $installInstructions -Encoding ASCII
Write-Success "Kurulum talimatları oluşturuldu"

# ZIP paketi oluştur
Write-Info "🗜️ ZIP paketi oluşturuluyor..."
$zipPath = Join-Path $OutputDir "$packageName.zip"
Compress-Archive -Path "$packageDir\*" -DestinationPath $zipPath -Force

# Dosya boyutlarını hesapla ve göster
$exeSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
$zipSize = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)

Write-Info "=" * 50
Write-Success "✅ Release build başarıyla tamamlandı!"
Write-Info "📊 Dosya Boyutları:"
Write-Info "   • Executable: $exeSize MB"
Write-Info "   • ZIP Paketi: $zipSize MB"
Write-Info "📁 Çıktı Dosyaları:"
Write-Info "   • Executable: $exePath"
Write-Info "   • Paket Klasörü: $packageDir"
Write-Info "   • ZIP Paketi: $zipPath"
Write-Info "=" * 50

# Desktop shortcut oluşturma seçeneği
$createShortcut = Read-Host "Desktop kısayolu oluşturulsun mu? (y/N)"
if ($createShortcut -eq "y" -or $createShortcut -eq "Y") {
    try {
        $WshShell = New-Object -comObject WScript.Shell
        $Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\Uzman Raporu.lnk")
        $Shortcut.TargetPath = $exePath
        $Shortcut.WorkingDirectory = Split-Path $exePath
        $Shortcut.Description = "Uzman Raporu - Bilirkişi Sicil Arama Sistemi"
        $Shortcut.Save()
        Write-Success "Desktop kısayolu oluşturuldu"
    } catch {
        Write-Warning "Desktop kısayolu oluşturulamadı: $($_.Exception.Message)"
    }
}

Write-Success "🎉 İşlem tamamlandı! Uygulamanız kullanıma hazır."
