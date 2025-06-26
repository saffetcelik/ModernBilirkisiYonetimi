using System;
using System.IO;
using System.Windows;

namespace BilirkisiMasaustu
{
    /// <summary>
    /// App.xaml etkileşim mantığı
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handling
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // --- YENİ BAŞLANGIÇ MANTIĞI ---
            Window startupWindow;
            try
            {
                var settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");

                if (File.Exists(settingsPath))
                {
                    var savedPath = File.ReadAllText(settingsPath).Trim();

                    if (!string.IsNullOrEmpty(savedPath) && File.Exists(savedPath))
                    {
                        // Kaydedilmiş kurul var, direkt ana pencereyi aç
                        startupWindow = new MainWindow(savedPath);
                    }
                    else
                    {
                        // Ayar dosyası var ama geçersiz, seçim penceresini aç
                        startupWindow = new KuruluSecimWindow();
                    }
                }
                else
                {
                    // Ayar dosyası yok, seçim penceresini aç
                    startupWindow = new KuruluSecimWindow();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda güvenli moda geç ve seçim penceresini aç
                System.Diagnostics.Debug.WriteLine($"Başlangıç hatası: {ex.Message}");
                startupWindow = new KuruluSecimWindow();
            }

            // Uygulamanın ana penceresini ayarla ve göster
            this.MainWindow = startupWindow;
            this.MainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Beklenmeyen hata oluştu:\n{e.Exception.Message}",
                "Uygulama Hatası", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Kritik hata oluştu:\n{ex.Message}",
                    "Kritik Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
