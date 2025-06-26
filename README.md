# Uzman Raporu: Yeni Nesil Bilirkişi Platformu

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet) ![Platform](https://img.shields.io/badge/Platform-Windows-blue) ![License](https://img.shields.io/badge/License-Apache%202.0-yellowgreen)

Uzman Raporu, adli ve hukuki süreçlerinizde en doğru bilirkişiyi saniyeler içinde bulmanızı sağlayan modern bir masaüstü uygulamasıdır. Gelişmiş arama ve filtreleme yetenekleriyle, Türkiye'nin dört bir yanındaki binlerce uzman profiline anında erişim sunarak adaletin hızına hız katar.

![Uygulama Arayüzü](https://raw.githubusercontent.com/saffetcelik/ModernBilirkisiYonetimi/main/tanitim.gif)

---

## ✨ Özellikler

- **Gelişmiş Arama:** Uzmanlık alanı, şehir, ad veya sicil numarasına göre anında arama yapın.
- **Detaylı Filtreleme:** Sonuçları birden fazla kritere göre daraltarak en uygun uzmana ulaşın.
- **Profil Görüntüleme:** Bilirkişilerin sicil numarası, uzmanlık alanları ve diğer detaylarını tek ekranda görün.
- **Kurul Seçimi:**  Seçtiğiniz bilirkişi kuruluna göre programı kullanabilme özelliği
- **İstatistikler:** Sistemdeki verilere dayalı görsel istatistiklerle genel durumu analiz edin.
- **Modern Arayüz:** Kullanıcı dostu ve sezgisel WPF arayüzü ile verimli bir çalışma deneyimi.

---

## 🚀 Teknoloji Yığını

- **Framework:** .NET 8
- **Arayüz:** Windows Presentation Foundation (WPF)
- **Veri İşleme:** Newtonsoft.Json

---

## 🛠️ Kurulum ve Kullanım

### Kullanıcılar için (Çalıştırmaya Hazır Sürüm)

1.  Projenin **[Releases](https://github.com/saffetcelik/ModernBilirkisiYonetimi/releases)** sayfasından en son sürümü (`.zip`) indirin.
2.  İndirilen ZIP dosyasını bir klasöre çıkartın.
3.  `BilirkisiMasaustu.exe` dosyasına çift tıklayarak uygulamayı başlatın. Ek bir kurulum gerektirmez!

### Geliştiriciler için (Kaynak Koddan Çalıştırma)

1.  Bu repoyu klonlayın:
    ```sh
    git clone https://github.com/saffetcelik/ModernBilirkisiYonetimi.git
    ```
2.  Proje dizinine gidin:
    ```sh
    cd ModernBilirkisiYonetimi
    ```
3.  Gerekli paketleri geri yükleyin:
    ```sh
    dotnet restore
    ```
4.  Uygulamayı çalıştırın:
    ```sh
    dotnet run --project BilirkisiMasaustu.csproj
    ```

---

## 📦 Derleme (Publish)

Uygulamanın .NET bağımlılığı olmayan, diğer bilgisayarlarda doğrudan çalışabilen sürümünü oluşturmak için aşağıdaki komutu kullanabilirsiniz.

```powershell
dotnet publish BilirkisiMasaustu.csproj --configuration Release --runtime win-x64 --self-contained true
```

Eğer tüm dosyaları tek bir `.exe` içinde toplamak isterseniz:

```powershell
dotnet publish --configuration Release --runtime win-x64 --self-contained true /p:PublishSingleFile=true
```

Oluşturulan dosyalar `bin/Release/net8.0-windows/win-x64/publish/` dizininde yer alacaktır.

---

## 📄 Lisans

Bu proje **Apache 2.0 Lisansı** altında lisanslanmıştır. Detaylar için `LICENSE` dosyasına göz atın.
