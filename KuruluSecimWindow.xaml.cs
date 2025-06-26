using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BilirkisiMasaustu
{
    /// <summary>
    /// Bilirkişi kurulu seçim penceresi
    /// </summary>
    public partial class KuruluSecimWindow : Window
    {
        public string? SelectedJsonPath { get; private set; }
        public string? SelectedKuruluName { get; private set; }
        
        private List<KuruluInfo> _kurullar = new List<KuruluInfo>();

        public KuruluSecimWindow()
        {
            InitializeComponent();
            Loaded += KuruluSecimWindow_Loaded;
        }

        private async void KuruluSecimWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadKurullarAsync();
        }

        /// <summary>
        /// İller klasöründeki JSON dosyalarını okuyarak kurulları yükle
        /// </summary>
        private async Task LoadKurullarAsync()
        {
            try
            {
                LoadingOverlay.Visibility = Visibility.Visible;
                StatusText.Text = "Kurullar yükleniyor...";

                await Task.Delay(500); // UI için kısa gecikme

                var illerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "iller");
                
                if (!Directory.Exists(illerPath))
                {
                    MessageBox.Show("İller klasörü bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                var jsonFiles = Directory.GetFiles(illerPath, "*_bilirkisi_verileri.json");
                
                if (jsonFiles.Length == 0)
                {
                    MessageBox.Show("İller klasöründe bilirkişi veri dosyası bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                _kurullar.Clear();

                foreach (var filePath in jsonFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var kuruluInfo = ParseKuruluInfo(fileName, filePath);
                    if (kuruluInfo != null)
                    {
                        _kurullar.Add(kuruluInfo);
                    }
                }

                // Alfabetik sırala
                _kurullar = _kurullar.OrderBy(k => k.Title).ToList();

                // ListBox'a yükle
                KuruluListBox.ItemsSource = _kurullar;

                StatusText.Text = $"{_kurullar.Count} bilirkişi kurulu bulundu";
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                MessageBox.Show($"Kurullar yüklenirken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        /// <summary>
        /// Dosya adından kurulu bilgilerini çıkar
        /// </summary>
        private KuruluInfo? ParseKuruluInfo(string fileName, string filePath)
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

                return new KuruluInfo
                {
                    Title = $"{ilAdi} Bilirkişi Kurulu",
                    Subtitle = $"Dosya: {Path.GetFileName(filePath)}",
                    FilePath = filePath,
                    IlAdi = ilAdi
                };
            }
            catch
            {
                return null;
            }
        }

        private void KuruluListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedKurulu = KuruluListBox.SelectedItem as KuruluInfo;
            SelectButton.IsEnabled = selectedKurulu != null;
            
            if (selectedKurulu != null)
            {
                StatusText.Text = $"Seçilen: {selectedKurulu.Title}";
            }
            else
            {
                StatusText.Text = "Lütfen bir bilirkişi kurulu seçin";
            }
        }

        private void KuruluListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (KuruluListBox.SelectedItem != null)
            {
                SelectButton_Click(sender, e);
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedKurulu = KuruluListBox.SelectedItem as KuruluInfo;
            if (selectedKurulu != null)
            {
                SelectedJsonPath = selectedKurulu.FilePath;
                SelectedKuruluName = selectedKurulu.Title;

                try
                {
                    // Dosya varlığını kontrol et
                    if (!File.Exists(selectedKurulu.FilePath))
                    {
                        MessageBox.Show($"JSON dosyası bulunamadı:\n{selectedKurulu.FilePath}",
                            "Dosya Bulunamadı", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Kurul seçimini kaydet
                    SaveKuruluSelection(selectedKurulu.FilePath);



                    // Ana pencereyi aç
                    var mainWindow = new MainWindow(selectedKurulu.FilePath);
                    mainWindow.Show();

                    // Bu pencereyi kapat
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ana pencere açılırken hata oluştu:\n\nHata: {ex.Message}\n\nDosya: {selectedKurulu.FilePath}\n\nDetay:\n{ex.StackTrace}",
                        "Ana Pencere Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Kurulu bilgi modeli
    /// </summary>
    public class KuruluInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string IlAdi { get; set; } = string.Empty;
    }
}
