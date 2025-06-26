# Version Bump Script
# Bu script version numarasını günceller ve release için hazırlar

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("major", "minor", "patch")]
    [string]$BumpType,
    
    [string]$CustomVersion = "",
    [switch]$DryRun = $false,
    [switch]$CreateTag = $false,
    [switch]$PushTag = $false
)

# Renkli çıktı fonksiyonları
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

Write-Info "🔢 Version Bump Script"
Write-Info "Bump Type: $BumpType"
if ($DryRun) { Write-Warning "DRY RUN MODE - Değişiklikler uygulanmayacak" }
Write-Info "=" * 50

# Mevcut version'ı oku
$csprojPath = "BilirkisiMasaustu.csproj"
if (-not (Test-Path $csprojPath)) {
    Write-Error "BilirkisiMasaustu.csproj dosyası bulunamadı!"
    exit 1
}

$content = Get-Content $csprojPath -Raw
$versionMatch = [regex]::Match($content, '<Version>(.*?)</Version>')

if (-not $versionMatch.Success) {
    Write-Error "Mevcut version bulunamadı!"
    exit 1
}

$currentVersion = $versionMatch.Groups[1].Value
Write-Info "Mevcut Version: $currentVersion"

# Yeni version hesapla
if ($CustomVersion) {
    $newVersion = $CustomVersion
    Write-Info "Özel version kullanılıyor: $newVersion"
} else {
    $versionParts = $currentVersion.Split('.')
    if ($versionParts.Length -ne 3) {
        Write-Error "Version formatı geçersiz! Beklenen format: x.y.z"
        exit 1
    }

    $major = [int]$versionParts[0]
    $minor = [int]$versionParts[1]
    $patch = [int]$versionParts[2]

    switch ($BumpType) {
        "major" {
            $major++
            $minor = 0
            $patch = 0
        }
        "minor" {
            $minor++
            $patch = 0
        }
        "patch" {
            $patch++
        }
    }

    $newVersion = "$major.$minor.$patch"
}

Write-Success "Yeni Version: $newVersion"

if ($DryRun) {
    Write-Warning "DRY RUN: Değişiklikler uygulanmadı"
    exit 0
}

# Proje dosyasını güncelle
Write-Info "📝 Proje dosyası güncelleniyor..."
$content = $content -replace '<AssemblyVersion>.*?</AssemblyVersion>', "<AssemblyVersion>$newVersion.0</AssemblyVersion>"
$content = $content -replace '<FileVersion>.*?</FileVersion>', "<FileVersion>$newVersion.0</FileVersion>"
$content = $content -replace '<Version>.*?</Version>', "<Version>$newVersion</Version>"

Set-Content $csprojPath $content -NoNewline
Write-Success "Proje dosyası güncellendi"

# CHANGELOG.md güncelle
Write-Info "📋 CHANGELOG.md güncelleniyor..."
if (Test-Path "CHANGELOG.md") {
    $changelogContent = Get-Content "CHANGELOG.md" -Raw
    $today = Get-Date -Format "yyyy-MM-dd"
    
    # [Unreleased] bölümünü yeni version ile değiştir
    $changelogContent = $changelogContent -replace '\[Unreleased\]', "[$newVersion] - $today"
    
    # Yeni [Unreleased] bölümü ekle
    $unreleasedSection = @"
## [Unreleased]

### Eklenen
### Değiştirilen
### Düzeltilen
### Kaldırılan

## [$newVersion] - $today
"@
    
    $changelogContent = $changelogContent -replace "## \[$newVersion\] - $today", $unreleasedSection
    
    Set-Content "CHANGELOG.md" $changelogContent -Encoding UTF8
    Write-Success "CHANGELOG.md güncellendi"
} else {
    Write-Warning "CHANGELOG.md bulunamadı"
}

# Git işlemleri
if (Get-Command git -ErrorAction SilentlyContinue) {
    Write-Info "📝 Git commit oluşturuluyor..."
    
    git add $csprojPath
    if (Test-Path "CHANGELOG.md") {
        git add "CHANGELOG.md"
    }
    
    git commit -m "chore: bump version to $newVersion"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Git commit oluşturuldu"
        
        if ($CreateTag) {
            Write-Info "🏷️ Git tag oluşturuluyor..."
            git tag "v$newVersion"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Git tag oluşturuldu: v$newVersion"
                
                if ($PushTag) {
                    Write-Info "📤 Tag push ediliyor..."
                    git push origin "v$newVersion"
                    
                    if ($LASTEXITCODE -eq 0) {
                        Write-Success "Tag başarıyla push edildi"
                        Write-Info "🚀 GitHub Actions otomatik olarak release oluşturacak"
                    } else {
                        Write-Error "Tag push edilemedi"
                    }
                }
            } else {
                Write-Error "Git tag oluşturulamadı"
            }
        }
    } else {
        Write-Error "Git commit oluşturulamadı"
    }
} else {
    Write-Warning "Git bulunamadı, version control işlemleri atlandı"
}

Write-Info "=" * 50
Write-Success "✅ Version bump işlemi tamamlandı!"
Write-Info "Eski Version: $currentVersion"
Write-Info "Yeni Version: $newVersion"

if ($CreateTag -and -not $PushTag) {
    Write-Info ""
    Write-Warning "Tag oluşturuldu ancak push edilmedi."
    Write-Info "Release oluşturmak için şu komutu çalıştırın:"
    Write-Info "git push origin v$newVersion"
}

Write-Info "=" * 50
