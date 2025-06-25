using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using BilirkisiMasaustu.Models;

namespace BilirkisiMasaustu.Services
{
    /// <summary>
    /// Favori bilirkişiler servisi
    /// </summary>
    public class FavoriService
    {
        private readonly string _favoriFilePath;
        private List<Bilirkisi> _favoriler = new List<Bilirkisi>();

        public FavoriService()
        {
            // Uygulama dizinindeki favori dosyası
            _favoriFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favoriler.json");
            LoadFavoriler();
        }

        /// <summary>
        /// Favori değişiklik eventi
        /// </summary>
        public event Action? FavorilerChanged;

        /// <summary>
        /// Tüm favorileri getir
        /// </summary>
        public List<Bilirkisi> GetFavoriler()
        {
            return _favoriler.ToList();
        }

        /// <summary>
        /// Bilirkişi favorilerde mi kontrol et
        /// </summary>
        public bool IsFavori(Bilirkisi bilirkisi)
        {
            return _favoriler.Any(f => f.SicilNo == bilirkisi.SicilNo);
        }

        /// <summary>
        /// Favoriye ekle
        /// </summary>
        public bool FavoriyeEkle(Bilirkisi bilirkisi)
        {
            if (IsFavori(bilirkisi))
                return false; // Zaten favoride

            _favoriler.Add(bilirkisi);
            SaveFavoriler();
            FavorilerChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Favoriden çıkar
        /// </summary>
        public bool FavoridenCikar(Bilirkisi bilirkisi)
        {
            var favori = _favoriler.FirstOrDefault(f => f.SicilNo == bilirkisi.SicilNo);
            if (favori == null)
                return false; // Favoride değil

            _favoriler.Remove(favori);
            SaveFavoriler();
            FavorilerChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Favori durumunu toggle et
        /// </summary>
        public bool ToggleFavori(Bilirkisi bilirkisi)
        {
            if (IsFavori(bilirkisi))
            {
                FavoridenCikar(bilirkisi);
                return false; // Favoriden çıkarıldı
            }
            else
            {
                FavoriyeEkle(bilirkisi);
                return true; // Favoriye eklendi
            }
        }

        /// <summary>
        /// Favorileri dosyadan yükle
        /// </summary>
        private void LoadFavoriler()
        {
            try
            {
                if (File.Exists(_favoriFilePath))
                {
                    var json = File.ReadAllText(_favoriFilePath);
                    _favoriler = JsonConvert.DeserializeObject<List<Bilirkisi>>(json) ?? new List<Bilirkisi>();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda boş liste
                _favoriler = new List<Bilirkisi>();
                System.Diagnostics.Debug.WriteLine($"Favori yükleme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Favorileri dosyaya kaydet
        /// </summary>
        private void SaveFavoriler()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_favoriler, Formatting.Indented);
                File.WriteAllText(_favoriFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Favori kaydetme hatası: {ex.Message}");
            }
        }

        /// <summary>
        /// Favori sayısı
        /// </summary>
        public int FavoriSayisi => _favoriler.Count;

        /// <summary>
        /// Tüm favorileri temizle
        /// </summary>
        public void TumFavorileriTemizle()
        {
            _favoriler.Clear();
            SaveFavoriler();
            FavorilerChanged?.Invoke();
        }
    }
}
