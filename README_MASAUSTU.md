# 🖥️ Bilirkişi Sicil Arama Sistemi - Masaüstü Uygulaması

**Excel VBA'dan Modern WPF Masaüstü Uygulamasına Dönüştürülmüş Bilirkişi Arama Sistemi**

## 📋 Özellikler

- ✅ **Masaüstü Uygulaması**: Localhost gerektirmez, doğrudan çalışır
- ✅ **Hızlı Arama**: Ad soyad ile bilirkişi arama (Excel VBA InStr mantığı)
- ✅ **Otomatik Kopyalama**: Sicil numarasını otomatik panoya kopyalama
- ✅ **Modern WPF UI**: Material Design ile güzel arayüz
- ✅ **Real-time Arama**: Yazarken otomatik arama (2+ karakter)
- ✅ **Favori Sistem**: Bilirkişileri favorilere ekleme/çıkarma
- ✅ **Hızlı Erişim**: Favori listesinden tek tıkla sicil kopyalama
- ✅ **Bildirim Sistemi**: Görsel bildirimler
- ✅ **İstatistikler**: İl ve meslek bazında dağılım
- ✅ **Türkçe Karakter Desteği**: Büyük/küçük harf duyarsız arama
- ✅ **1516 Kayıt**: bilirkisi_verileri_full.json dosyasından okur

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- .NET 8 SDK
- Windows 10/11

### Hızlı Başlangıç

1. **Projeyi derleyin**
   ```bash
   dotnet build BilirkisiMasaustu.csproj
   ```

2. **Uygulamayı çalıştırın**
   ```bash
   dotnet run --project BilirkisiMasaustu.csproj
   ```

3. **Veya EXE dosyasını oluşturun**
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained
   ```

## 📊 Veri Kaynağı

Uygulama `bilirkisi_verileri_full.json` dosyasından verileri okur:
- **1516 bilirkişi kaydı**
- **Sakarya Bölgesi verileri**
- **Excel'den dönüştürülmüş JSON formatı**

## 🎯 Kullanım

### Ana Ekran
1. **Ad soyad girin** (örn: "UFUK GÜNGÖR")
2. **2+ karakter yazınca otomatik arama başlar**
3. **Enter'a basın veya Sorgula butonuna tıklayın**
4. **Sonuçlardan birini seçin**
5. **Sicil numarası otomatik kopyalanır**
6. **Detay bilgileri sağda görünür**

### İstatistikler
- **İstatistik butonuna tıklayın**
- **İl bazında dağılım**
- **Meslek bazında dağılım**
- **Sistem bilgileri**

### Sicil Kopyalama
- **Bilirkişi seçildiğinde otomatik kopyalanır**
- **"Sicil Kopyala" butonuna manuel tıklayabilirsiniz**
- **Panoya kopyalanan sicil numarasını istediğiniz yere yapıştırın**

## 🆚 Excel VBA vs WPF Masaüstü

| Özellik | Excel VBA | WPF Masaüstü |
|---------|-----------|---------------|
| **Platform** | Sadece Windows + Excel | Windows (Excel gerektirmez) |
| **Performans** | Yavaş | ⚡ Çok hızlı |
| **Arayüz** | Basit | 🎨 Modern Material Design |
| **Arama** | Manuel | 🔄 Real-time otomatik |
| **Veri kapasitesi** | Excel sınırları | 🚀 Sınırsız JSON |
| **Güncelleme** | Manuel | 📁 JSON dosyası değiştir |
| **Dağıtım** | Excel dosyası | 📦 Tek EXE dosyası |

## 🔍 Arama Mantığı

Excel VBA'daki `InStr` fonksiyonu mantığı korunmuştur:

```vb
' VBA Kodu:
If InStr(1, Trim(ws.Cells(i, "B").Value), arananAdSoyad, vbTextCompare) > 0 Then
```

```csharp
// C# Karşılığı:
public bool MatchesSearch(string searchTerm)
{
    var normalizedSearch = NormalizeString(searchTerm.Trim());
    return NormalizedAdSoyad.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
}
```

## 📁 Proje Yapısı

```
BilirkisiMasaustu/
├── Models/
│   └── BilirkisiModel.cs          # Veri modelleri
├── Services/
│   └── BilirkisiDataService.cs    # JSON okuma ve arama servisi
├── MainWindow.xaml                # Ana pencere tasarımı
├── MainWindow.xaml.cs             # Ana pencere mantığı
├── StatisticsWindow.xaml          # İstatistik penceresi
├── StatisticsWindow.xaml.cs       # İstatistik mantığı
├── App.xaml                       # Uygulama kaynakları
├── App.xaml.cs                    # Uygulama başlangıcı
├── BilirkisiMasaustu.csproj       # Proje dosyası
└── bilirkisi_verileri_full.json   # Veri dosyası (1516 kayıt)
```

## 🎨 Arayüz Özellikleri

### Ana Pencere
- **Modern Material Design**
- **Responsive layout**
- **Real-time arama**
- **Otomatik sicil kopyalama**
- **Detaylı bilgi paneli**

### İstatistik Penceresi
- **Toplam bilirkişi sayısı**
- **İl bazında dağılım (Top 10)**
- **Meslek bazında dağılım (Top 10)**
- **Sistem bilgileri**

## 🔧 Geliştirme

### Yeni Özellik Ekleme
1. **Model güncellemesi**: `Models/BilirkisiModel.cs`
2. **Servis güncellemesi**: `Services/BilirkisiDataService.cs`
3. **UI güncellemesi**: `MainWindow.xaml` ve `MainWindow.xaml.cs`

### Veri Güncelleme
1. **Yeni Excel dosyasını JSON'a çevirin**
2. **`bilirkisi_verileri_full.json` dosyasını değiştirin**
3. **Uygulamayı yeniden başlatın**

## 📦 Dağıtım

### Tek EXE Dosyası Oluşturma
```bash
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

### Kurulum Paketi
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 🎉 Başarı!

Excel VBA programınız artık modern bir WPF masaüstü uygulamasına dönüştürüldü!

**Avantajlar:**
- 🖥️ **Masaüstü uygulaması** - Excel gerektirmez
- ⚡ **Çok daha hızlı** - JSON in-memory okuma
- 🎨 **Modern arayüz** - Material Design
- 🔄 **Real-time arama** - Yazarken otomatik
- 📋 **Otomatik kopyalama** - Sicil numarası
- 📊 **Gelişmiş istatistikler** - Görsel raporlar
- 🚀 **Kolay dağıtım** - Tek EXE dosyası

**Kullanım:**
1. Uygulamayı açın
2. Ad soyad yazın
3. Sonucu seçin
4. Sicil numarası otomatik kopyalanır!

Tamam şef! 🎯
