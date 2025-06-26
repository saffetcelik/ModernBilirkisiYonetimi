using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BilirkisiMasaustu.Models
{
    /// <summary>
    /// Bilirkişi veri modeli
    /// </summary>
    public class Bilirkisi
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("sicilNo")]
        public string SicilNo { get; set; } = string.Empty;

        [JsonProperty("adSoyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [JsonProperty("temelUzmanlikAlanlari")]
        public string TemelUzmanlikAlanlari { get; set; } = string.Empty;

        [JsonProperty("altUzmanlikAlanlari")]
        public string AltUzmanlikAlanlari { get; set; } = string.Empty;

        [JsonProperty("il")]
        public string Il { get; set; } = string.Empty;

        [JsonProperty("meslegi")]
        public string Meslegi { get; set; } = string.Empty;

        [JsonProperty("tumBilgiler")]
        public string TumBilgiler { get; set; } = string.Empty;

        [JsonProperty("excelSatirNo")]
        public int ExcelSatirNo { get; set; }

        /// <summary>
        /// Arama için normalize edilmiş ad soyad
        /// </summary>
        public string NormalizedAdSoyad => NormalizeString(AdSoyad);

        /// <summary>
        /// Listbox'ta gösterilecek metin
        /// </summary>
        public string DisplayText => $"{AdSoyad} - Sicil: {SicilNo} - {Il}";

        /// <summary>
        /// ToString override - ListBox'ta gösterilecek
        /// </summary>
        public override string ToString()
        {
            return AdSoyad;
        }

        /// <summary>
        /// Türkçe karakterleri normalize et
        /// </summary>
        private static string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input
                .Replace('ç', 'c').Replace('Ç', 'C')
                .Replace('ğ', 'g').Replace('Ğ', 'G')
                .Replace('ı', 'i').Replace('I', 'I')
                .Replace('İ', 'I').Replace('i', 'i')
                .Replace('ö', 'o').Replace('Ö', 'O')
                .Replace('ş', 's').Replace('Ş', 'S')
                .Replace('ü', 'u').Replace('Ü', 'U');
        }

        /// <summary>
        /// Arama kontrolü (Excel VBA InStr mantığı)
        /// </summary>
        public bool MatchesSearch(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return false;

            var normalizedSearch = NormalizeString(searchTerm.Trim());
            return NormalizedAdSoyad.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// JSON dosyasından okunan metadata
    /// </summary>
    public class BilirkisiMetadata
    {
        [JsonProperty("kaynak")]
        public string Kaynak { get; set; } = string.Empty;

        [JsonProperty("olusturmaTarihi")]
        public string OlusturmaTarihi { get; set; } = string.Empty;

        [JsonProperty("toplamKayit")]
        public int ToplamKayit { get; set; }

        [JsonProperty("aciklama")]
        public string Aciklama { get; set; } = string.Empty;

        [JsonProperty("surunNo")]
        public string SurunNo { get; set; } = string.Empty;

        [JsonProperty("excelToplamSatir")]
        public int ExcelToplamSatir { get; set; }

        [JsonProperty("islenenSatir")]
        public int IslenenSatir { get; set; }
    }

    /// <summary>
    /// JSON dosyasının ana yapısı
    /// </summary>
    public class BilirkisiData
    {
        [JsonProperty("metadata")]
        public BilirkisiMetadata Metadata { get; set; } = new BilirkisiMetadata();

        [JsonProperty("bilirkisiler")]
        public List<Bilirkisi> Bilirkisiler { get; set; } = new List<Bilirkisi>();
    }
}
