# 🚀 Release Rehberi - Uzman Raporu

Bu rehber, Uzman Raporu uygulamasının performanslı release sürümlerini oluşturma ve GitHub'da otomatik release yapma sürecini açıklar.

## 📦 Yerel Build (Manuel)

### Hızlı Build
```powershell
.\build-release-final.ps1 -Version "1.0.1"
```

### Detaylı Build (Eski Script)
```powershell
.\build-release.ps1 -Version "1.0.1" -Clean -SkipTests
```

### Version Güncelleme
```powershell
.\version-bump.ps1 -BumpType patch -CreateTag -PushTag
```

## 🤖 GitHub Actions (Otomatik)

### Otomatik Release Oluşturma

GitHub Actions sistemi 2 şekilde çalışır:

#### 1. Tag Push ile Otomatik Release
```bash
# Version'ı güncelle ve tag oluştur
git add .
git commit -m "feat: yeni özellikler eklendi"
git tag v1.0.1
git push origin main
git push origin v1.0.1
```

#### 2. Manuel Workflow Tetikleme
1. GitHub repository'ye gidin
2. **Actions** sekmesine tıklayın
3. **Release Build** workflow'unu seçin
4. **Run workflow** butonuna tıklayın
5. Version numarasını girin (örn: v1.0.1)
6. **Run workflow** ile başlatın

### GitHub Actions Ne Yapar?

1. ✅ .NET 8 SDK kurulumu
2. ✅ Dependencies restore
3. ✅ Solution build
4. ✅ Test çalıştırma (varsa)
5. ✅ Performanslı publish (self-contained)
6. ✅ Release paketi oluşturma
7. ✅ ZIP dosyası hazırlama
8. ✅ GitHub Release oluşturma
9. ✅ Dosyaları release'e ekleme

## 📋 Release Özellikleri

### Oluşturulan Dosyalar
- `UzmanRaporu-v{version}-win-x64.zip` - Tam paket
- İçinde:
  - `BilirkisiMasaustu.exe` - Ana uygulama
  - `iller/` - Veri dosyaları
  - `README.md` - Proje açıklaması
  - `LICENSE` - Lisans dosyası
  - `KURULUM.txt` - Kurulum talimatları

### Teknik Özellikler
- **Self-contained**: .NET 8 runtime dahil
- **Single-file**: Tek executable dosya
- **Optimized**: ReadyToRun ve compression
- **Portable**: Herhangi bir Windows PC'de çalışır
- **Size**: ~76 MB executable, ~73 MB ZIP

## 🔧 Yapılandırma Dosyaları

### `.github/workflows/release.yml`
GitHub Actions workflow dosyası. Otomatik build ve release sürecini yönetir.

### `BilirkisiMasaustu.csproj`
Proje dosyası. Performans optimizasyonları ve metadata içerir:
- PublishSingleFile: true
- SelfContained: true
- PublishReadyToRun: true
- EnableCompressionInSingleFile: true

### `build-release-final.ps1`
Yerel build script'i. Manuel release oluşturmak için kullanılır.

### `version-bump.ps1`
Version güncelleme script'i. Semantic versioning ile otomatik güncelleme.

## 📝 Release Süreci Adımları

### 1. Geliştirme Tamamlandığında
```bash
git add .
git commit -m "feat: yeni özellik eklendi"
git push origin main
```

### 2. Release Hazırlığı
```bash
# Version'ı güncelle (patch/minor/major)
.\version-bump.ps1 -BumpType patch

# Veya manuel version
.\version-bump.ps1 -CustomVersion "1.2.0"
```

### 3. Tag Oluştur ve Push Et
```bash
git tag v1.2.0
git push origin v1.2.0
```

### 4. GitHub Actions Otomatik Çalışır
- Build işlemi başlar
- Release oluşturulur
- Dosyalar yüklenir

### 5. Release Yayınlanır
- GitHub Releases sayfasında görünür
- İndirme linkleri hazır olur
- Changelog otomatik oluşturulur

## 🎯 Sonuç

Bu sistem sayesinde:
- ✅ Tek komutla release oluşturabilirsiniz
- ✅ GitHub'da otomatik release yayınlanır
- ✅ Performanslı, optimize edilmiş executable
- ✅ Self-contained, bağımsız çalışan uygulama
- ✅ Profesyonel release notları
- ✅ Otomatik versiyonlama

## 📞 İletişim

- **Geliştirici**: Saffet Çelik
- **E-posta**: iletisim@saffetcelik.com
- **GitHub**: github.com/saffetcelik
