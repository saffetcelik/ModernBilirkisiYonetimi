using System;
using System.Linq;
using System.Windows;
using BilirkisiMasaustu.Services;

namespace BilirkisiMasaustu
{
    /// <summary>
    /// StatisticsWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class StatisticsWindow : Window
    {
        private readonly BilirkisiDataService _dataService;

        public StatisticsWindow(BilirkisiDataService dataService)
        {
            InitializeComponent();
            _dataService = dataService;
            
            Loaded += StatisticsWindow_Loaded;
        }

        private void StatisticsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }

        /// <summary>
        /// İstatistikleri yükle ve göster
        /// </summary>
        private void LoadStatistics()
        {
            try
            {
                var stats = _dataService.GetStatistics();
                var metadata = _dataService.GetMetadata();
                var allBilirkisiler = _dataService.GetAllBilirkisiler();

                // Genel sayılar
                TotalCountLabel.Text = _dataService.TotalCount.ToString();

                // İl dağılımı
                var ilDagilim = allBilirkisiler
                    .GroupBy(b => b.Il)
                    .Select(g => new { 
                        Il = g.Key, 
                        Sayi = g.Count(),
                        Yuzde = $"{(g.Count() * 100.0 / _dataService.TotalCount):F1}%"
                    })
                    .OrderByDescending(x => x.Sayi)
                    .Take(10)
                    .ToList();

                CityCountLabel.Text = allBilirkisiler.GroupBy(b => b.Il).Count().ToString();
                CityListView.ItemsSource = ilDagilim;

                // Meslek dağılımı
                var meslekDagilim = allBilirkisiler
                    .GroupBy(b => b.Meslegi)
                    .Select(g => new { 
                        Meslek = g.Key, 
                        Sayi = g.Count(),
                        Yuzde = $"{(g.Count() * 100.0 / _dataService.TotalCount):F1}%"
                    })
                    .OrderByDescending(x => x.Sayi)
                    .Take(10)
                    .ToList();

                ProfessionCountLabel.Text = allBilirkisiler.GroupBy(b => b.Meslegi).Count().ToString();
                ProfessionListView.ItemsSource = meslekDagilim;

                // Sistem bilgileri
                SourceLabel.Text = metadata.Kaynak;
                CreationLabel.Text = DateTime.TryParse(metadata.OlusturmaTarihi, out var creationDate) 
                    ? creationDate.ToString("dd.MM.yyyy HH:mm") 
                    : metadata.OlusturmaTarihi;

                // JSON dosya boyutu
                var jsonPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bilirkisi_verileri_full.json");
                if (System.IO.File.Exists(jsonPath))
                {
                    var fileInfo = new System.IO.FileInfo(jsonPath);
                    var sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                    FileSizeLabel.Text = $"{sizeInMB:F2} MB";
                }
                else
                {
                    FileSizeLabel.Text = "Dosya bulunamadı";
                }

                VersionLabel.Text = metadata.SurunNo ?? "1.0.0";
                DescriptionLabel.Text = metadata.Aciklama;
                UpdateTimeLabel.Text = $"Son güncelleme: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"İstatistik yükleme hatası:\n{ex.Message}", "Hata", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Kapat butonu
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
