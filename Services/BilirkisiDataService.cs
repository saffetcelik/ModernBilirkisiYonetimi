using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BilirkisiMasaustu.Models;

namespace BilirkisiMasaustu.Services
{
    /// <summary>
    /// Bilirkişi veri servisi - JSON dosyasından veri okuma ve arama
    /// </summary>
    public class BilirkisiDataService
    {
        private List<Bilirkisi> _bilirkisiler = new List<Bilirkisi>();
        private BilirkisiMetadata _metadata = new BilirkisiMetadata();
        private string _jsonFilePath;

        public BilirkisiDataService()
        {
            // Varsayılan olarak uygulama dizinindeki JSON dosyasını kullan
            _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bilirkisi_verileri_full.json");
        }

        /// <summary>
        /// JSON dosya yolunu dinamik olarak ayarla
        /// </summary>
        public void SetJsonFilePath(string jsonFilePath)
        {
            _jsonFilePath = jsonFilePath;
        }

        /// <summary>
        /// Mevcut JSON dosya yolunu getir
        /// </summary>
        public string GetJsonFilePath()
        {
            return _jsonFilePath;
        }

        /// <summary>
        /// JSON dosyasından verileri yükle
        /// </summary>
        public async Task<bool> LoadDataAsync()
        {
            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    throw new FileNotFoundException($"JSON dosyası bulunamadı: {_jsonFilePath}");
                }

                var jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var data = JsonConvert.DeserializeObject<BilirkisiData>(jsonContent);

                if (data?.Bilirkisiler != null)
                {
                    _bilirkisiler = data.Bilirkisiler;
                    _metadata = data.Metadata ?? new BilirkisiMetadata();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Veri yükleme hatası: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ad soyad ile bilirkişi ara (Excel VBA InStr mantığı)
        /// </summary>
        public List<Bilirkisi> SearchBilirkisi(string adSoyad)
        {
            if (string.IsNullOrWhiteSpace(adSoyad))
                return new List<Bilirkisi>();

            return _bilirkisiler
                .Where(b => b.MatchesSearch(adSoyad))
                .OrderBy(b => b.AdSoyad)
                .ToList();
        }

        /// <summary>
        /// Sicil numarası ile bilirkişi bul
        /// </summary>
        public Bilirkisi? GetBySicilNo(string sicilNo)
        {
            if (string.IsNullOrWhiteSpace(sicilNo))
                return null;

            return _bilirkisiler.FirstOrDefault(b => 
                b.SicilNo.Equals(sicilNo.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Tüm bilirkişileri getir
        /// </summary>
        public List<Bilirkisi> GetAllBilirkisiler()
        {
            return _bilirkisiler.ToList();
        }

        /// <summary>
        /// Metadata bilgilerini getir
        /// </summary>
        public BilirkisiMetadata GetMetadata()
        {
            return _metadata;
        }

        /// <summary>
        /// İstatistik bilgilerini getir
        /// </summary>
        public object GetStatistics()
        {
            var ilDagilim = _bilirkisiler
                .GroupBy(b => b.Il)
                .Select(g => new { Il = g.Key, Sayi = g.Count() })
                .OrderByDescending(x => x.Sayi)
                .Take(10)
                .ToList();

            var meslekDagilim = _bilirkisiler
                .GroupBy(b => b.Meslegi)
                .Select(g => new { Meslek = g.Key, Sayi = g.Count() })
                .OrderByDescending(x => x.Sayi)
                .Take(10)
                .ToList();

            return new
            {
                ToplamBilirkisi = _bilirkisiler.Count,
                Metadata = _metadata,
                IlDagilim = ilDagilim,
                MeslekDagilim = meslekDagilim,
                JsonDosyaYolu = _jsonFilePath,
                JsonDosyaBoyutu = File.Exists(_jsonFilePath) ? new FileInfo(_jsonFilePath).Length : 0
            };
        }

        /// <summary>
        /// Veri yüklenmiş mi kontrol et
        /// </summary>
        public bool IsDataLoaded => _bilirkisiler.Count > 0;

        /// <summary>
        /// Toplam kayıt sayısı
        /// </summary>
        public int TotalCount => _bilirkisiler.Count;
    }
}
