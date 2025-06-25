// Bilirkişi Arama Sistemi - JavaScript
// Modern ES6+ JavaScript ile geliştirildi

class BilirkisiAramaApp {
    constructor() {
        this.apiBaseUrl = '/api/bilirkisi';
        this.secilenBilirkisi = null;
        this.init();
    }

    init() {
        // Event listeners
        this.setupEventListeners();
        
        // Sayfa yüklendiğinde istatistikleri yükle
        this.istatistikleriYukle();
        
        console.log('Bilirkişi Arama Sistemi başlatıldı');
    }

    setupEventListeners() {
        // Form submit
        document.getElementById('aramaForm').addEventListener('submit', (e) => {
            e.preventDefault();
            this.bilirkisiAra();
        });

        // Enter tuşu ile arama
        document.getElementById('adSoyadInput').addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.bilirkisiAra();
            }
        });

        // Input değişikliği (otomatik arama için)
        let timeout;
        document.getElementById('adSoyadInput').addEventListener('input', (e) => {
            clearTimeout(timeout);
            const value = e.target.value.trim();
            
            if (value.length >= 2) {
                timeout = setTimeout(() => {
                    this.bilirkisiAra();
                }, 500); // 500ms bekle
            } else {
                this.sonuclariTemizle();
            }
        });
    }

    async bilirkisiAra() {
        const adSoyad = document.getElementById('adSoyadInput').value.trim();
        const tamEslestirme = document.getElementById('tamEslestirme').checked;

        if (!adSoyad) {
            this.showAlert('Lütfen ad soyad girin', 'warning');
            return;
        }

        if (adSoyad.length < 2) {
            this.showAlert('En az 2 karakter girin', 'warning');
            return;
        }

        this.showLoading(true);

        try {
            const response = await fetch(`${this.apiBaseUrl}/ara`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    adSoyad: adSoyad,
                    tamEslestirme: tamEslestirme
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const sonuc = await response.json();
            this.sonuclariGoster(sonuc);

        } catch (error) {
            console.error('Arama hatası:', error);
            this.showAlert('Arama sırasında hata oluştu: ' + error.message, 'danger');
        } finally {
            this.showLoading(false);
        }
    }

    sonuclariGoster(sonuc) {
        const sonucAlani = document.getElementById('sonucAlani');
        const sonucMesaji = document.getElementById('sonucMesaji');
        const sonucListesi = document.getElementById('sonucListesi');

        // Sonuç alanını göster
        sonucAlani.style.display = 'block';
        sonucAlani.classList.add('fade-in');

        // Mesajı göster
        sonucMesaji.textContent = sonuc.mesaj;
        sonucMesaji.className = sonuc.basarili && sonuc.toplamSonuc > 0 
            ? 'alert alert-success' 
            : 'alert alert-info';

        // Listeyi temizle
        sonucListesi.innerHTML = '';

        if (sonuc.basarili && sonuc.bulunanlar && sonuc.bulunanlar.length > 0) {
            sonuc.bulunanlar.forEach((bilirkisi, index) => {
                const listItem = this.createBilirkisiListItem(bilirkisi, index);
                sonucListesi.appendChild(listItem);
            });

            // İlk sonucu otomatik seç
            if (sonuc.bulunanlar.length === 1) {
                this.bilirkisiSec(sonuc.bulunanlar[0], 0);
            }
        } else {
            sonucListesi.innerHTML = '<div class="p-3 text-center text-muted">Sonuç bulunamadı</div>';
            this.detayAlaniTemizle();
        }
    }

    createBilirkisiListItem(bilirkisi, index) {
        const listItem = document.createElement('div');
        listItem.className = 'list-group-item bilirkisi-item';
        listItem.onclick = () => this.bilirkisiSec(bilirkisi, index);

        listItem.innerHTML = `
            <div class="bilirkisi-name">${bilirkisi.adSoyad}</div>
            <div class="bilirkisi-sicil">Sicil No: ${bilirkisi.sicilNo}</div>
            <div class="bilirkisi-il">${bilirkisi.il}</div>
        `;

        return listItem;
    }

    bilirkisiSec(bilirkisi, index) {
        // Önceki seçimi temizle
        document.querySelectorAll('.bilirkisi-item').forEach(item => {
            item.classList.remove('active', 'selected');
        });

        // Yeni seçimi işaretle
        const selectedItem = document.querySelectorAll('.bilirkisi-item')[index];
        if (selectedItem) {
            selectedItem.classList.add('active', 'selected');
        }

        this.secilenBilirkisi = bilirkisi;
        this.detaylariGoster(bilirkisi);
        
        // Sicil numarasını otomatik kopyala
        this.sicilKopyala(false); // Sessiz kopyalama
    }

    detaylariGoster(bilirkisi) {
        const detayAlani = document.getElementById('detayAlani');
        const kopyalaBtn = document.getElementById('kopyalaBtn');

        detayAlani.innerHTML = `
            <div class="sicil-highlight">
                <i class="fas fa-id-card me-2"></i>
                Sicil No: ${bilirkisi.sicilNo}
            </div>
            
            <div class="detay-item">
                <div class="detay-label">
                    <i class="fas fa-user me-2"></i>Ad Soyad
                </div>
                <div class="detay-value">${bilirkisi.adSoyad}</div>
            </div>

            <div class="detay-item">
                <div class="detay-label">
                    <i class="fas fa-map-marker-alt me-2"></i>İl
                </div>
                <div class="detay-value">${bilirkisi.il}</div>
            </div>

            <div class="detay-item">
                <div class="detay-label">
                    <i class="fas fa-briefcase me-2"></i>Mesleği
                </div>
                <div class="detay-value">${bilirkisi.meslegi}</div>
            </div>

            <div class="detay-item">
                <div class="detay-label">
                    <i class="fas fa-graduation-cap me-2"></i>Temel Uzmanlık Alanları
                </div>
                <div class="detay-value">${bilirkisi.temelUzmanlikAlanlari}</div>
            </div>

            <div class="detay-item">
                <div class="detay-label">
                    <i class="fas fa-star me-2"></i>Alt Uzmanlık Alanları
                </div>
                <div class="detay-value">${bilirkisi.altUzmanlikAlanlari}</div>
            </div>

            <div class="detay-item">
                <div class="detay-label">
                    <i class="fas fa-info-circle me-2"></i>Tüm Bilgiler
                </div>
                <div class="detay-value">${bilirkisi.tumBilgiler}</div>
            </div>
        `;

        detayAlani.classList.add('slide-in');
        kopyalaBtn.style.display = 'block';
    }

    sicilKopyala(showMessage = true) {
        if (!this.secilenBilirkisi) {
            if (showMessage) {
                this.showAlert('Önce bir bilirkişi seçin', 'warning');
            }
            return;
        }

        const sicilNo = this.secilenBilirkisi.sicilNo;
        
        // Clipboard API kullan
        if (navigator.clipboard && window.isSecureContext) {
            navigator.clipboard.writeText(sicilNo).then(() => {
                if (showMessage) {
                    this.showAlert(`Sicil numarası kopyalandı: ${sicilNo}`, 'success');
                }
                this.animateButton('kopyalaBtn');
            }).catch(err => {
                console.error('Kopyalama hatası:', err);
                this.fallbackCopy(sicilNo, showMessage);
            });
        } else {
            this.fallbackCopy(sicilNo, showMessage);
        }
    }

    fallbackCopy(text, showMessage) {
        // Fallback yöntemi
        const textArea = document.createElement('textarea');
        textArea.value = text;
        textArea.style.position = 'fixed';
        textArea.style.left = '-999999px';
        textArea.style.top = '-999999px';
        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();
        
        try {
            document.execCommand('copy');
            if (showMessage) {
                this.showAlert(`Sicil numarası kopyalandı: ${text}`, 'success');
            }
            this.animateButton('kopyalaBtn');
        } catch (err) {
            console.error('Fallback kopyalama hatası:', err);
            if (showMessage) {
                this.showAlert('Kopyalama başarısız', 'danger');
            }
        } finally {
            document.body.removeChild(textArea);
        }
    }

    animateButton(buttonId) {
        const button = document.getElementById(buttonId);
        if (button) {
            button.classList.add('copy-success');
            setTimeout(() => {
                button.classList.remove('copy-success');
            }, 1000);
        }
    }

    async istatistikleriYukle() {
        try {
            const response = await fetch(`${this.apiBaseUrl}/istatistikler`);
            if (response.ok) {
                this.istatistikler = await response.json();
            }
        } catch (error) {
            console.error('İstatistik yükleme hatası:', error);
        }
    }

    async istatistikleriGoster() {
        try {
            // Güncel istatistikleri al
            const response = await fetch(`${this.apiBaseUrl}/istatistikler`);
            if (!response.ok) {
                throw new Error('İstatistikler alınamadı');
            }

            const istatistikler = await response.json();
            this.istatistikModalGoster(istatistikler);

        } catch (error) {
            console.error('İstatistik hatası:', error);
            this.showAlert('İstatistikler yüklenirken hata oluştu', 'danger');
        }
    }

    istatistikModalGoster(istatistikler) {
        const modalIcerik = document.getElementById('istatistikIcerik');

        modalIcerik.innerHTML = `
            <div class="row mb-4">
                <div class="col-md-6">
                    <div class="stat-card">
                        <div class="stat-number">${istatistikler.toplamBilirkisi}</div>
                        <div class="stat-label">Toplam Bilirkişi</div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="stat-card">
                        <div class="stat-number">${istatistikler.ilBazindaDagilim?.length || 0}</div>
                        <div class="stat-label">Farklı İl</div>
                    </div>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6">
                    <h6><i class="fas fa-map-marker-alt me-2"></i>İl Bazında Dağılım</h6>
                    <div class="list-group">
                        ${istatistikler.ilBazindaDagilim?.map(item => `
                            <div class="list-group-item d-flex justify-content-between align-items-center">
                                ${item.il}
                                <span class="badge bg-primary rounded-pill">${item.sayi}</span>
                            </div>
                        `).join('') || '<p class="text-muted">Veri yok</p>'}
                    </div>
                </div>
                <div class="col-md-6">
                    <h6><i class="fas fa-briefcase me-2"></i>Meslek Bazında Dağılım</h6>
                    <div class="list-group">
                        ${istatistikler.meslekBazindaDagilim?.map(item => `
                            <div class="list-group-item d-flex justify-content-between align-items-center">
                                <small>${item.meslek}</small>
                                <span class="badge bg-success rounded-pill">${item.sayi}</span>
                            </div>
                        `).join('') || '<p class="text-muted">Veri yok</p>'}
                    </div>
                </div>
            </div>

            <div class="mt-4 p-3 bg-light rounded">
                <h6><i class="fas fa-info-circle me-2"></i>Sistem Bilgileri</h6>
                <p class="mb-1"><strong>Kaynak:</strong> ${istatistikler.metadata?.kaynak || 'Bilinmiyor'}</p>
                <p class="mb-1"><strong>Son Güncelleme:</strong> ${istatistikler.sonGuncelleme || 'Bilinmiyor'}</p>
                <p class="mb-0"><strong>Açıklama:</strong> ${istatistikler.metadata?.aciklama || 'Bilgi yok'}</p>
            </div>
        `;

        // Modal'ı göster
        const modal = new bootstrap.Modal(document.getElementById('istatistikModal'));
        modal.show();
    }

    showLoading(show) {
        const yuklemeDurumu = document.getElementById('yuklemeDurumu');
        yuklemeDurumu.style.display = show ? 'block' : 'none';
    }

    showAlert(message, type = 'info') {
        // Geçici alert göster
        const alertDiv = document.createElement('div');
        alertDiv.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        alertDiv.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        alertDiv.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(alertDiv);

        // 3 saniye sonra otomatik kapat
        setTimeout(() => {
            if (alertDiv.parentNode) {
                alertDiv.remove();
            }
        }, 3000);
    }

    sonuclariTemizle() {
        const sonucAlani = document.getElementById('sonucAlani');
        sonucAlani.style.display = 'none';
        this.detayAlaniTemizle();
    }

    detayAlaniTemizle() {
        const detayAlani = document.getElementById('detayAlani');
        const kopyalaBtn = document.getElementById('kopyalaBtn');

        detayAlani.innerHTML = `
            <p class="text-muted text-center">
                <i class="fas fa-arrow-left me-2"></i>
                Soldan bir bilirkişi seçin
            </p>
        `;

        kopyalaBtn.style.display = 'none';
        this.secilenBilirkisi = null;
    }
}

// Global fonksiyonlar (HTML'den çağrılabilir)
let app;

document.addEventListener('DOMContentLoaded', function() {
    app = new BilirkisiAramaApp();
});

function bilirkisiAra(event) {
    if (event) event.preventDefault();
    if (app) app.bilirkisiAra();
}

function sicilKopyala() {
    if (app) app.sicilKopyala(true);
}

function istatistikleriGoster() {
    if (app) app.istatistikleriGoster();
}
