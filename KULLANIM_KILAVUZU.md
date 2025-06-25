# 📖 Bilirkişi Sicil Arama Sistemi - Kullanım Kılavuzu

## 🎯 Hızlı Başlangıç

### 1. Uygulamayı Açın
- **Geliştirme ortamında**: `dotnet run --project BilirkisiMasaustu.csproj`
- **EXE dosyası**: `bin\Release\net8.0-windows\win-x64\publish\BilirkisiMasaustu.exe`

### 2. Bilirkişi Arayın
1. **Ad soyad kutusuna yazın** (örn: "UFUK GÜNGÖR")
2. **2+ karakter yazınca otomatik arama başlar**
3. **Enter'a basın veya "Sorgula" butonuna tıklayın**

### 3. Sonucu Seçin
1. **Sol panelde bulunan sonuçlardan birini tıklayın**
2. **Sicil numarası otomatik panoya kopyalanır**
3. **Sağ panelde detay bilgiler görünür**

### 4. Sicil Numarasını veya Şablonu Kullanın
- **Sicil No**: Otomatik kopyalanan sicil numarasını istediğiniz yere yapıştırın
- **Mahkeme Şablonu**: "Şablon Kopyala" butonuna tıklayarak hazır mahkeme metnini kopyalayın
- **Manuel Kopyalama**: "Sicil Kopyala" veya "Şablon Kopyala" butonlarına tıklayın

## 📄 Yeni Özellik: Mahkeme Şablonu

### Şablon Formatı
Bilirkişi seçtiğinizde otomatik olarak mahkeme tutanağı için hazır şablon oluşturulur:

```
BİLİRKİŞİ: [Ad Soyad], [Uzmanlık Kodları] -, tarafları tanımaz, bilirkişiliğe engel hali yok. Usulen yemini yaptırıldı.
```

### Örnek Şablon
```
BİLİRKİŞİ: UFUK GÜNGÖR, [43.01, 43.02, 43.05, 45.03, 45.04, 46.01] -, tarafları tanımaz, bilirkişiliğe engel hali yok. Usulen yemini yaptırıldı.
```

### Kullanım
1. **Bilirkişi seçin** - Şablon otomatik oluşturulur
2. **"Şablon Kopyala" butonuna tıklayın**
3. **Mahkeme tutanağına yapıştırın**

## 🔍 Arama Özellikleri

### Otomatik Arama
- **2+ karakter yazınca arama başlar**
- **500ms bekleyip otomatik arar**
- **Türkçe karakter desteği var**

### Arama Mantığı
- **Excel VBA InStr mantığı korunmuştur**
- **Büyük/küçük harf duyarsız**
- **Kısmi eşleşme (contains)**

### Örnek Aramalar
```
"UFUK" → UFUK GÜNGÖR bulur
"güngör" → UFUK GÜNGÖR bulur  
"ufuk güngör" → UFUK GÜNGÖR bulur
"recep" → RECEP IŞIK bulur
```

## 📊 İstatistikler

### İstatistik Penceresini Açın
1. **"İstatistik" butonuna tıklayın**
2. **Sistem bilgilerini görün**

### Gösterilen Bilgiler
- **Toplam bilirkişi sayısı**: 1516
- **Farklı il sayısı**
- **Farklı meslek sayısı**
- **İl bazında dağılım (Top 10)**
- **Meslek bazında dağılım (Top 10)**
- **JSON dosya bilgileri**

## 🎨 Arayüz Rehberi

### Ana Pencere
```
┌─────────────────────────────────────────────────────────────────────┐
│ 🔍 Bilirkişi Sicil Arama Sistemi                                  │
│ Masaüstü Uygulaması - 1516 Kayıt                                  │
├─────────────────────────────────────────────────────────────────────┤
│ Ad Soyad: [_______________] [🔍 Sorgula] [📊 İstatistik]            │
├─────────────────────┬───────────────────────────────────────────────┤
│ 📋 Arama Sonuçları  │ ℹ️ Detay Bilgiler    [📋 Sicil] [📄 Şablon]   │
│                     │                                               │
│ • UFUK GÜNGÖR       │ 🆔 Sicil No: 33                               │
│   Sicil: 33         │                                               │
│   SAKARYA/HENDEK    │ 👤 Ad Soyad: UFUK GÜNGÖR                      │
│                     │ 📍 İl: SAKARYA / HENDEK                       │
│ • ÖZCAN KARAVAR     │ 💼 Meslek: MUHASEBE VE FİNANS...              │
│   Sicil: 48         │ 🎓 Temel Uzmanlık: 43 MUHASEBE...             │
│   KOCAELİ/İZMİT     │ ⭐ Alt Uzmanlık: 43.01 GENEL...               │
│                     │ ℹ️ Tüm Bilgiler: Sicil No: 33...               │
│                     │ 📊 Excel Satır No: 2                          │
│                     ├───────────────────────────────────────────────┤
│                     │ 📄 Mahkeme Şablonu                            │
│                     │                                               │
│                     │ BİLİRKİŞİ: UFUK GÜNGÖR, [43.01, 43.02,       │
│                     │ 43.05, 45.03, 45.04, 46.01] -, tarafları     │
│                     │ tanımaz, bilirkişiliğe engel hali yok.        │
│                     │ Usulen yemini yaptırıldı.                     │
└─────────────────────┴───────────────────────────────────────────────┘
│ ✅ 2 sonuç bulundu - bilirkisi_verileri_full.json            v1.0.0│
└─────────────────────────────────────────────────────────────────────┘
```

### Renk Kodları
- **Mavi**: Ana renkler, butonlar
- **Yeşil**: Başarı mesajları, kopyala butonu
- **Sarı**: Sicil numarası vurgusu
- **Gri**: Yardımcı metinler

## 🔧 Sorun Giderme

### Uygulama Açılmıyor
1. **.NET 8 yüklü mü kontrol edin**
2. **Windows 10/11 kullanıyor musunuz?**
3. **Antivirus yazılımı engelliyor olabilir**

### Veri Yüklenmiyor
1. **`bilirkisi_verileri_full.json` dosyası var mı?**
2. **Dosya bozuk olabilir, yeniden kopyalayın**
3. **Dosya boyutu ~2MB olmalı**

### Arama Çalışmıyor
1. **En az 2 karakter yazın**
2. **Türkçe karakterler sorun yaratabilir**
3. **Tam ad yazmaya çalışın**

### Sicil Kopyalanmıyor
1. **Önce bir sonuç seçin**
2. **"Sicil Kopyala" butonuna tıklayın**
3. **Clipboard izinleri kontrol edin**

### Şablon Kopyalanmıyor
1. **Önce bir bilirkişi seçin**
2. **"Şablon Kopyala" butonuna tıklayın**
3. **Şablon alanında metin var mı kontrol edin**

## 📁 Dosya Yapısı

### Gerekli Dosyalar
```
BilirkisiMasaustu.exe           # Ana uygulama
bilirkisi_verileri_full.json    # Veri dosyası (1516 kayıt)
*.dll                           # .NET kütüphaneleri
```

### Veri Dosyası Formatı
```json
{
  "metadata": {
    "kaynak": "Sakarya_Bilirkisiler_20250625_103228.xlsx",
    "toplamKayit": 1516,
    "aciklama": "Sakarya Bilirkişi Sicil Verileri"
  },
  "bilirkisiler": [
    {
      "id": 1,
      "sicilNo": "33",
      "adSoyad": "UFUK GÜNGÖR",
      "temelUzmanlikAlanlari": "43 MUHASEBE...",
      "altUzmanlikAlanlari": "43.01 GENEL MUHASEBE...",
      "il": "SAKARYA / HENDEK",
      "meslegi": "MUHASEBE VE FİNANS MÜDÜRÜ"
    }
  ]
}
```

## 🚀 Performans İpuçları

### Hızlı Arama
- **Kısa kelimeler yazın** (2-5 karakter)
- **Tam ad yerine sadece ad veya soyad**
- **Türkçe karakter kullanmayın**

### Bellek Kullanımı
- **Uygulama ~50MB RAM kullanır**
- **1516 kayıt in-memory yüklenir**
- **Arama çok hızlıdır (milisaniye)**

## 📞 Destek

### Sık Sorulan Sorular

**S: Excel VBA ile aynı sonuçları veriyor mu?**
A: Evet, aynı InStr mantığı kullanılıyor.

**S: Yeni veri nasıl eklenir?**
A: JSON dosyasını güncelleyin ve uygulamayı yeniden başlatın.

**S: Başka bilgisayarda çalışır mı?**
A: Evet, publish klasörünü kopyalayın.

**S: İnternet gerekiyor mu?**
A: Hayır, tamamen offline çalışır.

### İletişim
- **Geliştirici**: ASP.NET Core 8 + WPF
- **Versiyon**: 1.0.0
- **Tarih**: 25.06.2025

## 🎉 Başarılı Kullanım!

Artık Excel VBA programınızın modern masaüstü versiyonunu kullanabilirsiniz!

**Avantajlar:**
- ⚡ **10x daha hızlı** arama
- 🎨 **Modern arayüz**
- 📋 **Otomatik kopyalama**
- 🔄 **Real-time arama**
- 📊 **Gelişmiş istatistikler**

Tamam şef! 🎯
