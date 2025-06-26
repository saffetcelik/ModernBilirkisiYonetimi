using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BilirkisiMasaustu.Models;
using BilirkisiMasaustu.Services;

namespace BilirkisiMasaustu
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly BilirkisiDataService _dataService;
        private readonly FavoriService _favoriService;
        private Bilirkisi? _selectedBilirkisi;
        private Bilirkisi? _selectedProfessionBilirkisi;
        private List<Bilirkisi> _currentResults = new List<Bilirkisi>();
        private List<Bilirkisi> _currentProfessionBilirkisiler = new List<Bilirkisi>();
        private List<dynamic> _allProfessions = new List<dynamic>();
        private List<string> _allIller = new List<string>();
        private Dictionary<string, List<string>> _ilIlceMap = new Dictionary<string, List<string>>();
        private string _currentKuruluName = "Bilinmiyor";
        private string? _jsonPathToLoad = null; // Yüklenecek JSON dosyasının yolunu tutar
        private bool _isDataLoaded = false;

        public MainWindow()
        {
            _dataService = new BilirkisiDataService();
            _favoriService = new FavoriService();
            InitializeComponent();

            // Placeholder'ları başlangıçta görünür yap
            if (SearchPlaceholder != null)
                SearchPlaceholder.Visibility = Visibility.Visible;
            if (ProfessionSearchPlaceholder != null)
                ProfessionSearchPlaceholder.Visibility = Visibility.Visible;

            // Event handlers
            _favoriService.FavorilerChanged += OnFavorilerChanged;

            // Uygulama başladığında verileri yükle
            Loaded += MainWindow_Loaded; // Veri yükleme işlemini Loaded event'ine bağla
        }

        public MainWindow(string selectedJsonPath) : this()
        {
            // Yüklenecek dosya yolunu ayarla. Veri yüklemesi MainWindow_Loaded'da yapılacak.
            _jsonPathToLoad = selectedJsonPath;

            // Dosya yolundan kurul adını çıkar
            _currentKuruluName = ExtractKuruluNameFromPath(selectedJsonPath);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Eğer özel bir JSON yolu belirtilmemişse, varsayılanı yükle.
                // Belirtilmişse, o dosyayı yükle.
                await LoadDataAsync(_jsonPathToLoad);

                // Kurul bilgisini UI'da güncelle
                UpdateKuruluInfoOnStartup();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MainWindow constructor hatası:\n{ex.Message}\n\nDosya: {_jsonPathToLoad}",
                    "Constructor Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        /// <summary>
        /// Dosya adından kurulu adını çıkar
        /// </summary>
        private string ParseKuruluNameFromFileName(string fileName)
        {
            try
            {
                // Örnek: "adana_bilirkisi_verileri" -> "Adana Bilirkişi Kurulu"
                var parts = fileName.Replace("_bilirkisi_verileri", "").Split('_');
                var ilAdi = string.Join(" ", parts.Select(p =>
                    char.ToUpper(p[0]) + p.Substring(1).ToLower()));

                // Özel durumlar için düzeltmeler
                ilAdi = ilAdi switch
                {
                    "Diyarbakir" => "Diyarbakır",
                    "Istanbul" => "İstanbul",
                    "Izmir" => "İzmir",
                    _ => ilAdi
                };

                return $"{ilAdi} Bilirkişi Kurulu";
            }
            catch
            {
                return "Bilirkişi Kurulu";
            }
        }



        /// <summary>
        /// Belirtilen JSON dosyasından veya varsayılan yoldan verileri yükler.
        /// </summary>
        /// <param name="jsonPath">Yüklenecek JSON dosyasının yolu. Null ise varsayılan kullanılır.</param>
        private async Task LoadDataAsync(string? jsonPath = null)
        {
            // Eğer bir yol belirtilmişse, DataService'deki yolu güncelle
            if (!string.IsNullOrEmpty(jsonPath))
            {
                _dataService.SetJsonFilePath(jsonPath);
            }

            try
            {
                // UI elementlerinin null olmadığını kontrol et
                if (LoadingOverlay == null || StatusLabel == null)
                {
                    throw new InvalidOperationException("UI elementleri henüz yüklenmemiş");
                }

                LoadingOverlay.Visibility = Visibility.Visible;
                StatusLabel.Text = "JSON dosyası yükleniyor...";

                // DataService'in null olmadığını kontrol et
                if (_dataService == null)
                {
                    throw new InvalidOperationException("DataService başlatılmamış");
                }

                bool success = await _dataService.LoadDataAsync();

                if (success)
                {
                    var metadata = _dataService.GetMetadata();
                    var metadataKaynak = metadata?.Kaynak ?? "Bilinmeyen Kaynak";
                    StatusLabel.Text = $"✅ {_dataService.TotalCount} bilirkişi yüklendi - {metadataKaynak}";

                    // Title'ı güncelle
                    Title = $"Bilirkişi Sicil Arama - {_dataService.TotalCount} Kayıt";

                    // Kurulu bilgisini güncelle
                    UpdateKuruluInfo(_dataService.TotalCount);

                    // Favorileri yükle
                    LoadFavorites();

                    // Meslekleri yükle
                    LoadProfessions();

                    _isDataLoaded = true;
                }
                else
                {
                    StatusLabel.Text = "❌ Veri yükleme başarısız";
                    MessageBox.Show("JSON dosyası yüklenemedi!", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                if (StatusLabel != null)
                    StatusLabel.Text = "❌ Hata oluştu";

                MessageBox.Show($"Veri yükleme hatası:\n{ex.Message}\n\nDetay:\n{ex.StackTrace}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Arama textbox değiştiğinde otomatik arama
        /// </summary>
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Placeholder kontrolü
            UpdateSearchPlaceholder();

            if (!_isDataLoaded) return;

            var searchText = SearchTextBox.Text.Trim();

            if (searchText.Length >= 2)
            {
                PerformSearch(searchText);
            }
            else if (searchText.Length == 0)
            {
                ClearResults();
            }
        }

        private void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }

        private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSearchPlaceholder();
        }

        private void UpdateSearchPlaceholder()
        {
            if (SearchPlaceholder != null)
            {
                SearchPlaceholder.Visibility = string.IsNullOrEmpty(SearchTextBox.Text) ?
                    Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// Enter tuşu ile arama
        /// </summary>
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var searchText = SearchTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(searchText))
                {
                    PerformSearch(searchText);
                }
            }
        }

        /// <summary>
        /// Sorgula butonu
        /// </summary>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Lütfen ad soyad girin!", "Uyarı", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SearchTextBox.Focus();
                return;
            }

            PerformSearch(searchText);
        }

        /// <summary>
        /// Arama işlemini gerçekleştir
        /// </summary>
        private void PerformSearch(string searchText)
        {
            

            try
            {
                _currentResults = _dataService.SearchBilirkisi(searchText);
                DisplayResults(_currentResults);
                
                StatusLabel.Text = $"🔍 '{searchText}' için {_currentResults.Count} sonuç bulundu";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Arama hatası:\n{ex.Message}", "Hata", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Arama sonuçlarını göster
        /// </summary>
        private void DisplayResults(List<Bilirkisi> results)
        {
            ResultListBox.Items.Clear();
            
            if (results.Count == 0)
            {
                ResultCountLabel.Text = "📋 Sonuç bulunamadı";
                ClearDetailPanel();
                return;
            }

            ResultCountLabel.Text = $"📋 {results.Count} Sonuç Bulundu";

            foreach (var bilirkisi in results)
            {
                ResultListBox.Items.Add(bilirkisi);
            }

            // Tek sonuç varsa otomatik seç
            if (results.Count == 1)
            {
                ResultListBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Listbox'tan bilirkişi seçildiğinde
        /// </summary>
        private void ResultListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResultListBox.SelectedItem is Bilirkisi selectedBilirkisi)
            {
                _selectedBilirkisi = selectedBilirkisi;
                DisplayBilirkisiDetails(selectedBilirkisi);

                // Favoriye ekle butonunu güncelle
                UpdateFavoriteButton(selectedBilirkisi);

                // Sicil numarasını otomatik kopyala
                CopySicilToClipboard(false);
            }
        }

        /// <summary>
        /// Favoriler değiştiğinde
        /// </summary>
        private void OnFavorilerChanged()
        {
            LoadFavorites();
        }

        /// <summary>
        /// Favorileri yükle
        /// </summary>
        private void LoadFavorites()
        {
            try
            {
                if (_favoriService == null || FavoriteListBox == null || FavoriteCountLabel == null)
                    return;

                var favoriler = _favoriService.GetFavoriler();
                FavoriteListBox.Items.Clear();

                foreach (var favori in favoriler)
                {
                    FavoriteListBox.Items.Add(favori);
                }

                FavoriteCountLabel.Text = $"⭐ Favoriler ({favoriler.Count})";

                // Seçili bilirkişi varsa favori butonunu güncelle
                if (_selectedBilirkisi != null)
                {
                    UpdateFavoriteButton(_selectedBilirkisi);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadFavorites hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Favori butonunu güncelle
        /// </summary>
        private void UpdateFavoriteButton(Bilirkisi bilirkisi)
        {
            AddFavoriteButton.IsEnabled = true;

            if (_favoriService.IsFavori(bilirkisi))
            {
                AddFavoriteButton.Content = "⭐ Favoriden Çıkar";
                // Style'ı koruyarak sadece background rengini değiştir
                AddFavoriteButton.Background = new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
            }
            else
            {
                AddFavoriteButton.Content = "⭐ Favoriye Ekle";
                // Style'ı koruyarak sadece background rengini değiştir
                AddFavoriteButton.Background = new SolidColorBrush(Color.FromRgb(255, 193, 7)); // #FFC107
            }
        }

        /// <summary>
        /// Bilirkişi detaylarını göster
        /// </summary>
        private void DisplayBilirkisiDetails(Bilirkisi bilirkisi)
        {
            // Sicil no vurgusu
            SicilHighlight.Visibility = Visibility.Visible;
            SicilNoLabel.Text = $"🆔 Sicil No: {bilirkisi.SicilNo}";
            
            // Kopyala butonlarını aktif et
            CopyButton.IsEnabled = true;
            CopyTemplateButton.IsEnabled = true;

            // Detay panelini temizle ve yeniden oluştur
            DetailPanel.Children.Clear();

            // Ad Soyad
            AddDetailItem("👤 Ad Soyad", bilirkisi.AdSoyad);
            
            // İl
            AddDetailItem("📍 İl", bilirkisi.Il);
            
            // Meslek
            AddDetailItem("💼 Meslek", bilirkisi.Meslegi);
            
            // Temel Uzmanlık
            AddDetailItem("🎓 Temel Uzmanlık Alanları", bilirkisi.TemelUzmanlikAlanlari);
            
            // Alt Uzmanlık
            AddDetailItem("⭐ Alt Uzmanlık Alanları", bilirkisi.AltUzmanlikAlanlari);
            
            // Tüm Bilgiler
            AddDetailItem("ℹ️ Tüm Bilgiler", bilirkisi.TumBilgiler);
            
            // Excel Satır No
            AddDetailItem("📊 Excel Satır No", bilirkisi.ExcelSatirNo.ToString());

            // Şablonu oluştur ve göster
            GenerateTemplate(bilirkisi);
        }

        /// <summary>
        /// Detay item ekle
        /// </summary>
        private void AddDetailItem(string label, string value)
        {
            var border = new Border
            {
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var stackPanel = new StackPanel();

            var labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 3)
            };

            var valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.DarkSlateGray
            };

            stackPanel.Children.Add(labelBlock);
            stackPanel.Children.Add(valueBlock);
            border.Child = stackPanel;

            DetailPanel.Children.Add(border);
        }

        /// <summary>
        /// Şablon oluştur
        /// </summary>
        private void GenerateTemplate(Bilirkisi bilirkisi)
        {
            try
            {
                // Alt uzmanlık alanlarını temizle ve formatla
                var altUzmanliklar = FormatAltUzmanlikAlanlari(bilirkisi.AltUzmanlikAlanlari);

                // Şablon metni oluştur
                var template = $"BİLİRKİŞİ: {bilirkisi.AdSoyad}, [{altUzmanliklar}] -, tarafları tanımaz, bilirkişiliğe engel hali yok. Usulen yemini yaptırıldı.";

                TemplateTextBox.Text = template;
            }
            catch (Exception ex)
            {
                TemplateTextBox.Text = $"Şablon oluşturma hatası: {ex.Message}";
            }
        }

        /// <summary>
        /// Alt uzmanlık alanlarını formatla (kod + açıklama)
        /// </summary>
        private string FormatAltUzmanlikAlanlari(string altUzmanlikAlanlari)
        {
            if (string.IsNullOrWhiteSpace(altUzmanlikAlanlari))
                return "";

            try
            {
                // Regex ile kod + açıklama gruplarını bul
                // Örnek: "43.01 GENEL MUHASEBE 43.02 YÖNETİM MUHASEBESİ"
                var regex = new System.Text.RegularExpressions.Regex(@"(\d+\.\d+)\s+([A-ZÇĞIİÖŞÜ\s,\-\.]+?)(?=\s+\d+\.\d+|$)");
                var matches = regex.Matches(altUzmanlikAlanlari);

                var formatliAlanlar = new List<string>();

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (match.Groups.Count >= 3)
                    {
                        var kod = match.Groups[1].Value.Trim();
                        var aciklama = match.Groups[2].Value.Trim();

                        // Açıklamayı temizle
                        aciklama = aciklama.Replace("  ", " ").Trim();

                        // Kod + açıklama formatında ekle
                        formatliAlanlar.Add($"{kod} {aciklama}");
                    }
                }

                if (formatliAlanlar.Any())
                {
                    return string.Join(", ", formatliAlanlar);
                }

                // Alternatif yöntem: Manuel parsing
                return ParseAltUzmanlikManual(altUzmanlikAlanlari);
            }
            catch (Exception)
            {
                // Hata durumunda manuel parsing dene
                return ParseAltUzmanlikManual(altUzmanlikAlanlari);
            }
        }

        /// <summary>
        /// Manuel parsing yöntemi
        /// </summary>
        private string ParseAltUzmanlikManual(string altUzmanlikAlanlari)
        {
            try
            {
                var kelimeler = altUzmanlikAlanlari.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var sonuclar = new List<string>();
                var currentGroup = new List<string>();

                for (int i = 0; i < kelimeler.Length; i++)
                {
                    var kelime = kelimeler[i];

                    // Kod formatında mı kontrol et (örn: 43.01)
                    if (System.Text.RegularExpressions.Regex.IsMatch(kelime, @"^\d+\.\d+$"))
                    {
                        // Önceki grubu kaydet
                        if (currentGroup.Any())
                        {
                            sonuclar.Add(string.Join(" ", currentGroup));
                            currentGroup.Clear();
                        }

                        // Yeni grup başlat
                        currentGroup.Add(kelime);
                    }
                    else
                    {
                        // Açıklama kısmı
                        if (currentGroup.Any()) // Sadece kod varsa açıklama ekle
                        {
                            currentGroup.Add(kelime);
                        }
                    }
                }

                // Son grubu kaydet
                if (currentGroup.Any())
                {
                    sonuclar.Add(string.Join(" ", currentGroup));
                }

                if (sonuclar.Any())
                {
                    return string.Join(", ", sonuclar);
                }

                // Son çare: Orijinal metni kısalt
                return altUzmanlikAlanlari.Length > 200
                    ? altUzmanlikAlanlari.Substring(0, 200) + "..."
                    : altUzmanlikAlanlari;
            }
            catch (Exception)
            {
                // En son çare
                return altUzmanlikAlanlari.Length > 200
                    ? altUzmanlikAlanlari.Substring(0, 200) + "..."
                    : altUzmanlikAlanlari;
            }
        }

        /// <summary>
        /// Sicil kopyala butonu
        /// </summary>
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            CopySicilToClipboard(true);
        }

        /// <summary>
        /// Şablon kopyala butonu
        /// </summary>
        private void CopyTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            CopyTemplateToClipboard();
        }

        /// <summary>
        /// Şablonu panoya kopyala
        /// </summary>
        private void CopyTemplateToClipboard()
        {
            if (_selectedBilirkisi == null)
            {
                MessageBox.Show("Önce bir bilirkişi seçin!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var templateText = TemplateTextBox.Text;

            if (string.IsNullOrWhiteSpace(templateText) || templateText == "Bilirkişi seçin...")
            {
                MessageBox.Show("Şablon metni bulunamadı!", "Uyarı",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Clipboard.SetText(templateText);

                StatusLabel.Text = $"📄 Şablon kopyalandı: {_selectedBilirkisi.AdSoyad}";

                // Butonu geçici olarak yeşil yap
                var originalBrush = CopyTemplateButton.Background;
                CopyTemplateButton.Background = Brushes.Green;
                CopyTemplateButton.Content = "✅ Kopyalandı";

                // 2 saniye sonra eski haline döndür
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                timer.Tick += (s, args) =>
                {
                    CopyTemplateButton.Background = originalBrush;
                    CopyTemplateButton.Content = "📄 Şablon Kopyala";
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Şablon kopyalama hatası:\n{ex.Message}", "Hata",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sicil numarasını panoya kopyala
        /// </summary>
        private void CopySicilToClipboard(bool showMessage)
        {
            if (_selectedBilirkisi == null)
            {
                if (showMessage)
                {
                    MessageBox.Show("Önce bir bilirkişi seçin!", "Uyarı",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            try
            {
                Clipboard.SetText(_selectedBilirkisi.SicilNo);

                if (showMessage)
                {
                    StatusLabel.Text = $"📋 Sicil numarası kopyalandı: {_selectedBilirkisi.SicilNo}";

                    // Butonu geçici olarak yeşil yap
                    var originalBrush = CopyButton.Background;
                    CopyButton.Background = Brushes.Green;
                    CopyButton.Content = "✅ Kopyalandı";

                    // 1 saniye sonra eski haline döndür
                    var timer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(1)
                    };
                    timer.Tick += (s, args) =>
                    {
                        CopyButton.Background = originalBrush;
                        CopyButton.Content = "📋 Sicil Kopyala";
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            catch (Exception ex)
            {
                if (showMessage)
                {
                    MessageBox.Show($"Kopyalama hatası:\n{ex.Message}", "Hata",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Favoriye ekle/çıkar butonu
        /// </summary>
        private void AddFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBilirkisi == null)
            {
                ShowNotification("Önce bir bilirkişi seçin!", "#DC3545");
                return;
            }

            bool eklendi = _favoriService.ToggleFavori(_selectedBilirkisi);

            if (eklendi)
            {
                ShowNotification($"⭐ {_selectedBilirkisi.AdSoyad} favorilere eklendi!", "#28A745");
            }
            else
            {
                ShowNotification($"❌ {_selectedBilirkisi.AdSoyad} favorilerden çıkarıldı!", "#FFC107");
            }

            UpdateFavoriteButton(_selectedBilirkisi);
        }

        /// <summary>
        /// Favori listesinden çift tıklama
        /// </summary>
        private void FavoriteListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FavoriteListBox.SelectedItem is Bilirkisi selectedFavori)
            {
                // Sicil numarasını kopyala
                try
                {
                    Clipboard.SetText(selectedFavori.SicilNo);
                    ShowNotification($"📋 Sicil kopyalandı: {selectedFavori.SicilNo}", "#28A745");

                    // Detayları da göster
                    _selectedBilirkisi = selectedFavori;
                    DisplayBilirkisiDetails(selectedFavori);
                    UpdateFavoriteButton(selectedFavori);
                }
                catch (Exception ex)
                {
                    ShowNotification($"Kopyalama hatası: {ex.Message}", "#DC3545");
                }
            }
        }

        /// <summary>
        /// Tüm favorileri temizle
        /// </summary>
        private void ClearFavoritesButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tüm favorileri silmek istediğinizden emin misiniz?",
                "Favori Temizleme", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _favoriService.TumFavorileriTemizle();
                ShowNotification("🗑️ Tüm favoriler temizlendi!", "#DC3545");

                // Seçili bilirkişi varsa butonunu güncelle
                if (_selectedBilirkisi != null)
                {
                    UpdateFavoriteButton(_selectedBilirkisi);
                }
            }
        }

        /// <summary>
        /// İstatistik butonu
        /// </summary>
        private void StatsButton_Click(object sender, RoutedEventArgs e)
        {

            var statsWindow = new StatisticsWindow(_dataService);
            statsWindow.Owner = this;
            statsWindow.ShowDialog();
        }

        /// <summary>
        /// Sonuçları temizle
        /// </summary>
        private void ClearResults()
        {
            ResultListBox.Items.Clear();
            ResultCountLabel.Text = "📋 Arama Sonuçları";
            ClearDetailPanel();
            StatusLabel.Text = "Hazır";
        }

        /// <summary>
        /// Detay panelini temizle
        /// </summary>
        private void ClearDetailPanel()
        {
            DetailPanel.Children.Clear();
            DetailPanel.Children.Add(new TextBlock
            {
                Text = "👈 Soldan bir bilirkişi seçin",
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            });

            SicilHighlight.Visibility = Visibility.Collapsed;
            CopyButton.IsEnabled = false;
            CopyTemplateButton.IsEnabled = false;
            AddFavoriteButton.IsEnabled = false;
            TemplateTextBox.Text = "Bilirkişi seçin...";
            _selectedBilirkisi = null;
        }

        /// <summary>
        /// Meslekleri yükle
        /// </summary>
        private void LoadProfessions()
        {
            if (!_dataService.IsDataLoaded)
                return;

            try
            {
                var allBilirkisiler = _dataService.GetAllBilirkisiler();

                // Meslekleri grupla ve say
                var meslekGruplari = allBilirkisiler
                    .Where(b => !string.IsNullOrWhiteSpace(b.Meslegi))
                    .GroupBy(b => b.Meslegi.Trim())
                    .Select(g => new {
                        Meslek = g.Key,
                        Sayi = g.Count(),
                        Bilirkisiler = g.ToList()
                    })
                    .OrderByDescending(x => x.Sayi)
                    .ThenBy(x => x.Meslek)
                    .ToList();

                // Tüm meslekleri sakla (arama için)
                _allProfessions = meslekGruplari.Select(g => new {
                    Meslek = g.Meslek,
                    Sayi = g.Sayi
                }).Cast<dynamic>().ToList();

                // İl ve ilçe verilerini hazırla
                LoadIlIlceData();

                // Listeyi güncelle
                UpdateProfessionList(_allProfessions);
            }
            catch (Exception ex)
            {
                ShowNotification($"Meslek yükleme hatası: {ex.Message}", "#DC3545");
            }
        }

        /// <summary>
        /// İl ve ilçe verilerini yükle
        /// </summary>
        private void LoadIlIlceData()
        {
            if (!_dataService.IsDataLoaded)
                return;

            try
            {
                var allBilirkisiler = _dataService.GetAllBilirkisiler();

                // İl ve ilçe verilerini parse et
                _ilIlceMap.Clear();
                _allIller.Clear();

                foreach (var bilirkisi in allBilirkisiler)
                {
                    if (string.IsNullOrWhiteSpace(bilirkisi.Il))
                        continue;

                    var parts = bilirkisi.Il.Split('/');
                    if (parts.Length >= 1)
                    {
                        string il = parts[0].Trim();
                        string ilce = parts.Length > 1 ? parts[1].Trim() : "";

                        if (!_ilIlceMap.ContainsKey(il))
                        {
                            _ilIlceMap[il] = new List<string>();
                            _allIller.Add(il);
                        }

                        if (!string.IsNullOrEmpty(ilce) && !_ilIlceMap[il].Contains(ilce))
                        {
                            _ilIlceMap[il].Add(ilce);
                        }
                    }
                }

                // İl listesini sırala
                _allIller.Sort();
                foreach (var il in _ilIlceMap.Keys)
                {
                    _ilIlceMap[il].Sort();
                }

                // ComboBox'ları güncelle
                UpdateIlComboBox();
            }
            catch (Exception ex)
            {
                ShowNotification($"İl/İlçe yükleme hatası: {ex.Message}", "#DC3545");
            }
        }

        /// <summary>
        /// İl ComboBox'ını güncelle
        /// </summary>
        private void UpdateIlComboBox()
        {
            IlFilterComboBox.Items.Clear();
            IlFilterComboBox.Items.Add("Tüm İller");

            foreach (var il in _allIller)
            {
                IlFilterComboBox.Items.Add(il);
            }

            IlFilterComboBox.SelectedIndex = 0;

            // İlçe ComboBox'ını da başlat
            IlceFilterComboBox.Items.Clear();
            IlceFilterComboBox.Items.Add("Tüm İlçeler");
            IlceFilterComboBox.SelectedIndex = 0;
            IlceFilterComboBox.IsEnabled = false;
        }

        /// <summary>
        /// Meslek listesini güncelle
        /// </summary>
        private void UpdateProfessionList(List<dynamic> professions)
        {
            ProfessionListBox.Items.Clear();

            foreach (var profession in professions)
            {
                ProfessionListBox.Items.Add($"{profession.Meslek} ({profession.Sayi})");
            }

            ProfessionCountLabel.Text = $"💼 Meslekler ({professions.Count})";
        }

        /// <summary>
        /// Meslek seçildiğinde
        /// </summary>
        private void ProfessionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfessionListBox.SelectedItem != null)
            {
                string? selectedText = ProfessionListBox.SelectedItem.ToString();
                if (!string.IsNullOrEmpty(selectedText))
                {
                    // "(sayı)" kısmını çıkar
                    string selectedMeslek = selectedText.Substring(0, selectedText.LastIndexOf(" ("));
                    LoadProfessionBilirkisiler(selectedMeslek);
                }
            }
        }

        /// <summary>
        /// Seçilen mesleğe ait bilirkişileri yükle (il/ilçe filtresini de uygula)
        /// </summary>
        private void LoadProfessionBilirkisiler(string meslek)
        {
            try
            {
                var allBilirkisiler = _dataService.GetAllBilirkisiler();

                // Önce meslek filtresini uygula
                var filteredBilirkisiler = allBilirkisiler
                    .Where(b => b.Meslegi.Trim().Equals(meslek, StringComparison.OrdinalIgnoreCase));

                // İl filtresini uygula
                string selectedIl = IlFilterComboBox.SelectedItem?.ToString() ?? "";
                if (selectedIl != "Tüm İller" && !string.IsNullOrEmpty(selectedIl))
                {
                    filteredBilirkisiler = filteredBilirkisiler.Where(b =>
                        !string.IsNullOrWhiteSpace(b.Il) && b.Il.StartsWith(selectedIl));
                }

                // İlçe filtresini uygula
                string selectedIlce = IlceFilterComboBox.SelectedItem?.ToString() ?? "";
                if (selectedIlce != "Tüm İlçeler" && !string.IsNullOrEmpty(selectedIlce))
                {
                    filteredBilirkisiler = filteredBilirkisiler.Where(b =>
                        !string.IsNullOrWhiteSpace(b.Il) && b.Il.Contains($"/ {selectedIlce}"));
                }

                _currentProfessionBilirkisiler = filteredBilirkisiler
                    .OrderBy(b => b.AdSoyad)
                    .ToList();

                ProfessionBilirkisiListBox.Items.Clear();

                foreach (var bilirkisi in _currentProfessionBilirkisiler)
                {
                    ProfessionBilirkisiListBox.Items.Add(bilirkisi);
                }

                // Başlığı güncelle (il/ilçe bilgisi ile)
                string locationInfo = "";
                if (selectedIl != "Tüm İller" && !string.IsNullOrEmpty(selectedIl))
                {
                    locationInfo = $" - {selectedIl}";
                    if (selectedIlce != "Tüm İlçeler" && !string.IsNullOrEmpty(selectedIlce))
                    {
                        locationInfo += $"/{selectedIlce}";
                    }
                }

                ProfessionBilirkisiCountLabel.Text = $"👥 {meslek}{locationInfo} ({_currentProfessionBilirkisiler.Count})";

                // Detay alanını temizle
                ClearProfessionDetailPanel();
            }
            catch (Exception ex)
            {
                ShowNotification($"Bilirkişi yükleme hatası: {ex.Message}", "#DC3545");
            }
        }

        /// <summary>
        /// Meslek sekmesinde bilirkişi seçildiğinde
        /// </summary>
        private void ProfessionBilirkisiListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfessionBilirkisiListBox.SelectedItem is Bilirkisi selectedBilirkisi)
            {
                _selectedProfessionBilirkisi = selectedBilirkisi;
                DisplayProfessionBilirkisiDetails(selectedBilirkisi);
            }
        }

        /// <summary>
        /// Meslek sekmesinde bilirkişi çift tıklandığında
        /// </summary>
        private void ProfessionBilirkisiListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProfessionBilirkisiListBox.SelectedItem is Bilirkisi selectedBilirkisi)
            {
                // Sicil numarasını kopyala
                try
                {
                    Clipboard.SetText(selectedBilirkisi.SicilNo);
                    ShowNotification($"📋 Sicil kopyalandı: {selectedBilirkisi.SicilNo}", "#28A745");
                }
                catch (Exception ex)
                {
                    ShowNotification($"Kopyalama hatası: {ex.Message}", "#DC3545");
                }
            }
        }

        /// <summary>
        /// Meslek sekmesi sicil kopyala butonu
        /// </summary>
        private void ProfessionCopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProfessionBilirkisi == null)
            {
                ShowNotification("Önce bir bilirkişi seçin!", "#DC3545");
                return;
            }

            try
            {
                Clipboard.SetText(_selectedProfessionBilirkisi.SicilNo);
                ShowNotification($"📋 Sicil kopyalandı: {_selectedProfessionBilirkisi.SicilNo}", "#28A745");

                // Butonu geçici olarak yeşil yap
                var originalBrush = ProfessionCopyButton.Background;
                ProfessionCopyButton.Background = Brushes.Green;
                ProfessionCopyButton.Content = "✅ Kopyalandı";

                // 1 saniye sonra eski haline döndür
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                timer.Tick += (s, args) =>
                {
                    ProfessionCopyButton.Background = originalBrush;
                    ProfessionCopyButton.Content = "📋 Sicil Kopyala";
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                ShowNotification($"Kopyalama hatası: {ex.Message}", "#DC3545");
            }
        }

        /// <summary>
        /// Meslek sekmesinde bilirkişi detaylarını göster
        /// </summary>
        private void DisplayProfessionBilirkisiDetails(Bilirkisi bilirkisi)
        {
            // Sicil no vurgusu
            ProfessionSicilHighlight.Visibility = Visibility.Visible;
            ProfessionSicilNoLabel.Text = $"🆔 Sicil No: {bilirkisi.SicilNo}";

            // Kopyala butonunu aktif et
            ProfessionCopyButton.IsEnabled = true;

            // Detay panelini temizle ve yeniden oluştur
            ProfessionDetailPanel.Children.Clear();

            // Ad Soyad
            AddProfessionDetailItem("👤 Ad Soyad", bilirkisi.AdSoyad);

            // İl
            AddProfessionDetailItem("📍 İl", bilirkisi.Il);

            // Meslek
            AddProfessionDetailItem("💼 Meslek", bilirkisi.Meslegi);

            // Temel Uzmanlık
            AddProfessionDetailItem("🎓 Temel Uzmanlık Alanları", bilirkisi.TemelUzmanlikAlanlari);

            // Alt Uzmanlık
            AddProfessionDetailItem("⭐ Alt Uzmanlık Alanları", bilirkisi.AltUzmanlikAlanlari);

            // Excel Satır No
            AddProfessionDetailItem("📊 Excel Satır No", bilirkisi.ExcelSatirNo.ToString());
        }

        /// <summary>
        /// Meslek sekmesi detay item ekle
        /// </summary>
        private void AddProfessionDetailItem(string label, string value)
        {
            var border = new Border
            {
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var stackPanel = new StackPanel();

            var labelBlock = new TextBlock
            {
                Text = label,
                FontWeight = FontWeights.SemiBold,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 3)
            };

            var valueBlock = new TextBlock
            {
                Text = value,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.DarkSlateGray
            };

            stackPanel.Children.Add(labelBlock);
            stackPanel.Children.Add(valueBlock);
            border.Child = stackPanel;

            ProfessionDetailPanel.Children.Add(border);
        }

        /// <summary>
        /// Meslek detay panelini temizle
        /// </summary>
        private void ClearProfessionDetailPanel()
        {
            ProfessionDetailPanel.Children.Clear();
            ProfessionDetailPanel.Children.Add(new TextBlock
            {
                Text = "👈 Soldan bir bilirkişi seçin",
                FontSize = 14,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20)
            });

            ProfessionSicilHighlight.Visibility = Visibility.Collapsed;
            ProfessionCopyButton.IsEnabled = false;
            _selectedProfessionBilirkisi = null;
        }

        /// <summary>
        /// Bildirim göster
        /// </summary>
        private void ShowNotification(string message, string backgroundColor = "#28A745")
        {
            try
            {
                NotificationText.Text = message;

                // Renk ayarla
                var color = (Color)ColorConverter.ConvertFromString(backgroundColor);
                ((Border)NotificationOverlay.Child).Background = new SolidColorBrush(color);

                // Göster
                NotificationOverlay.Visibility = Visibility.Visible;

                // 3 saniye sonra gizle
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3)
                };
                timer.Tick += (s, args) =>
                {
                    NotificationOverlay.Visibility = Visibility.Collapsed;
                    timer.Stop();
                };
                timer.Start();
            }
            catch (Exception ex)
            {
                // Bildirim gösteremezse status bar'a yaz
                StatusLabel.Text = message;
                System.Diagnostics.Debug.WriteLine($"Bildirim hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Meslek arama textbox değiştiğinde
        /// </summary>
        private void ProfessionSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Placeholder kontrolü
            UpdateProfessionSearchPlaceholder();

            // Lokasyon filtreleme metodunu çağır (arama da dahil)
            FilterProfessionsByLocation();
        }

        private void ProfessionSearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateProfessionSearchPlaceholder();
        }

        private void ProfessionSearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateProfessionSearchPlaceholder();
        }

        private void UpdateProfessionSearchPlaceholder()
        {
            if (ProfessionSearchPlaceholder != null)
            {
                ProfessionSearchPlaceholder.Visibility = string.IsNullOrEmpty(ProfessionSearchTextBox.Text) ?
                    Visibility.Visible : Visibility.Hidden;
            }
        }

        /// <summary>
        /// İl filtresi değiştiğinde
        /// </summary>
        private void IlFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IlFilterComboBox.SelectedItem != null)
            {
                string selectedIl = IlFilterComboBox.SelectedItem.ToString() ?? "";

                // İlçe ComboBox'ını güncelle
                IlceFilterComboBox.Items.Clear();
                IlceFilterComboBox.Items.Add("Tüm İlçeler");

                if (selectedIl != "Tüm İller" && _ilIlceMap.ContainsKey(selectedIl))
                {
                    IlceFilterComboBox.IsEnabled = true;
                    foreach (var ilce in _ilIlceMap[selectedIl])
                    {
                        IlceFilterComboBox.Items.Add(ilce);
                    }
                }
                else
                {
                    IlceFilterComboBox.IsEnabled = false;
                }

                IlceFilterComboBox.SelectedIndex = 0;

                // Meslek listesini filtrele
                FilterProfessionsByLocation();
            }
        }

        /// <summary>
        /// İlçe filtresi değiştiğinde
        /// </summary>
        private void IlceFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterProfessionsByLocation();
        }

        /// <summary>
        /// Lokasyon filtresine göre meslekleri filtrele
        /// </summary>
        private void FilterProfessionsByLocation()
        {
            if (_allProfessions == null || _allProfessions.Count == 0)
                return;

            try
            {
                string selectedIl = IlFilterComboBox.SelectedItem?.ToString() ?? "";
                string selectedIlce = IlceFilterComboBox.SelectedItem?.ToString() ?? "";

                var allBilirkisiler = _dataService.GetAllBilirkisiler();

                // Lokasyon filtresini uygula
                var filteredBilirkisiler = allBilirkisiler.AsEnumerable();

                if (selectedIl != "Tüm İller" && !string.IsNullOrEmpty(selectedIl))
                {
                    filteredBilirkisiler = filteredBilirkisiler.Where(b =>
                        !string.IsNullOrWhiteSpace(b.Il) && b.Il.StartsWith(selectedIl));
                }

                if (selectedIlce != "Tüm İlçeler" && !string.IsNullOrEmpty(selectedIlce))
                {
                    filteredBilirkisiler = filteredBilirkisiler.Where(b =>
                        !string.IsNullOrWhiteSpace(b.Il) && b.Il.Contains($"/ {selectedIlce}"));
                }

                // Meslek arama filtresini de uygula
                string searchText = ProfessionSearchTextBox.Text?.Trim().ToLowerInvariant() ?? "";

                // Meslekleri grupla ve say
                var meslekGruplari = filteredBilirkisiler
                    .Where(b => !string.IsNullOrWhiteSpace(b.Meslegi))
                    .Where(b => string.IsNullOrEmpty(searchText) ||
                               b.Meslegi.ToLowerInvariant().Contains(searchText))
                    .GroupBy(b => b.Meslegi.Trim())
                    .Select(g => new {
                        Meslek = g.Key,
                        Sayi = g.Count()
                    })
                    .OrderByDescending(x => x.Sayi)
                    .ThenBy(x => x.Meslek)
                    .Cast<dynamic>()
                    .ToList();

                UpdateProfessionList(meslekGruplari);
            }
            catch (Exception ex)
            {
                ShowNotification($"Filtreleme hatası: {ex.Message}", "#DC3545");
            }
        }

        /// <summary>
        /// Bilirkişi kurulunu değiştir butonu click event
        /// </summary>
        private async void ChangeKuruluButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var kuruluSecimWindow = new KuruluSecimWindow();
                var result = kuruluSecimWindow.ShowDialog();

                if (result == true)
                {
                    if (!string.IsNullOrEmpty(kuruluSecimWindow.SelectedJsonPath))
                    {
                        // Loading göster
                        if (LoadingOverlay != null)
                            LoadingOverlay.Visibility = Visibility.Visible;
                        if (StatusLabel != null)
                            StatusLabel.Text = "Yeni kurulu verileri yükleniyor...";

                        // Yeni JSON dosya yolunu ayarla
                        _dataService?.SetJsonFilePath(kuruluSecimWindow.SelectedJsonPath);
                        _currentKuruluName = kuruluSecimWindow.SelectedKuruluName ?? "Bilirkişi Kurulu";

                        // Kurul seçimini kaydet
                        SaveKuruluSelection(kuruluSecimWindow.SelectedJsonPath);

                        // Verileri yeniden yükle
                        await LoadDataAsync();

                        ShowNotification($"{_currentKuruluName} başarıyla yüklendi!", "#28A745");
                    }
                    else
                    {
                        ShowNotification("Kurul seçimi yapılmadı!", "#FFC107");
                    }
                }
                else
                {
                    // Kullanıcı Cancel'a bastı veya pencereyi kapattı
                    ShowNotification("Kurul değiştirme işlemi iptal edildi.", "#6C757D");
                }
            }
            catch (Exception ex)
            {
                if (LoadingOverlay != null)
                    LoadingOverlay.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Kurulu değiştirirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Kurul seçimini kaydet
        /// </summary>
        private void SaveKuruluSelection(string filePath)
        {
            try
            {
                var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
                File.WriteAllText(settingsPath, filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Kurul seçimi kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Kurul seçimini temizle - bir sonraki açılışta kurul seçim ekranı gösterilir
        /// </summary>
        private void ClearKuruluSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Kaydedilmiş kurul seçimi temizlenecek.\nBir sonraki açılışta kurul seçim ekranı gösterilecek.\n\nDevam etmek istiyor musunuz?",
                    "Kurul Seçimini Temizle",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
                    if (File.Exists(settingsPath))
                    {
                        File.Delete(settingsPath);
                        ShowNotification("Kurul seçimi temizlendi. Bir sonraki açılışta kurul seçim ekranı gösterilecek.", "#28A745");
                    }
                    else
                    {
                        ShowNotification("Temizlenecek kurul seçimi bulunamadı.", "#FFC107");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Kurul seçimi temizlenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Uygulama başlangıcında kurul bilgisini güncelle (veri yüklenmeden önce)
        /// </summary>
        private void UpdateKuruluInfoOnStartup()
        {
            try
            {
                var kuruluName = _currentKuruluName ?? "Bilirkişi Kurulu";

                // Sol taraftaki TextBlock'ı güncelle
                if (KuruluInfoTextBlock != null)
                {
                    KuruluInfoTextBlock.Text = kuruluName;
                }

                // Ortadaki label'ı geçici olarak güncelle
                if (KuruluInfoLabel != null)
                {
                    KuruluInfoLabel.Text = $"{kuruluName} - Yükleniyor...";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateKuruluInfoOnStartup hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Kurulu bilgisini güncelle
        /// </summary>
        private void UpdateKuruluInfo(int kayitSayisi)
        {
            try
            {
                var kuruluName = _currentKuruluName ?? "Bilirkişi Kurulu";

                // Ortadaki label'ı güncelle
                if (KuruluInfoLabel != null)
                {
                    KuruluInfoLabel.Text = $"{kuruluName} - {kayitSayisi} Kayıt";
                }

                // Sol taraftaki TextBlock'ı güncelle
                if (KuruluInfoTextBlock != null)
                {
                    KuruluInfoTextBlock.Text = kuruluName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateKuruluInfo hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Dosya yolundan kurul adını çıkarır
        /// </summary>
        private string ExtractKuruluNameFromPath(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return "Bilirkişi Kurulu";

                var fileName = Path.GetFileNameWithoutExtension(filePath);

                // Örnek: "adana_bilirkisi_verileri" -> "Adana Bilirkişi Kurulu"
                var parts = fileName.Replace("_bilirkisi_verileri", "").Split('_');
                var ilAdi = string.Join(" ", parts.Select(p =>
                    char.ToUpper(p[0]) + p.Substring(1).ToLower()));

                // Özel durumlar için düzeltmeler
                ilAdi = ilAdi switch
                {
                    "Diyarbakir" => "Diyarbakır",
                    "Istanbul" => "İstanbul",
                    "Izmir" => "İzmir",
                    _ => ilAdi
                };

                return $"{ilAdi} Bilirkişi Kurulu";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractKuruluNameFromPath hatası: {ex.Message}");
                return "Bilirkişi Kurulu";
            }
        }

        #region Hakkında Kartı Event Handlers

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AboutOverlay.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AboutButton_Click hatası: {ex.Message}");
            }
        }

        private void CloseAboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AboutOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseAboutButton_Click hatası: {ex.Message}");
            }
        }

        private void AboutOverlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Overlay'e tıklandığında kartı kapat (arka plana tıklama)
                if (e.Source == AboutOverlay)
                {
                    AboutOverlay.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"AboutOverlay_MouseLeftButtonDown hatası: {ex.Message}");
            }
        }

        private void EmailLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mailto:iletisim@saffetcelik.com",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EmailLink_Click hatası: {ex.Message}");
                // Email açılamazsa, adresi panoya kopyala
                try
                {
                    Clipboard.SetText("iletisim@saffetcelik.com");
                    ShowNotification("Email adresi panoya kopyalandı!");
                }
                catch
                {
                    // Sessizce geç
                }
            }
        }

        private void GitHubLink_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/saffetcelik",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GitHubLink_Click hatası: {ex.Message}");
                // URL açılamazsa, adresi panoya kopyala
                try
                {
                    Clipboard.SetText("https://github.com/saffetcelik");
                    ShowNotification("GitHub adresi panoya kopyalandı!");
                }
                catch
                {
                    // Sessizce geç
                }
            }
        }

        #endregion
    }
}
