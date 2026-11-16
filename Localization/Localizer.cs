using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace AvaloniaAppCrypto.Localization
{
    /// <summary>
    /// Small runtime localizer for the two supported UI languages.
    /// Bind it from XAML with <c>{Binding Loc[Key]}</c>; changing the
    /// language raises the indexer notification so every binding refreshes.
    /// </summary>
    public sealed class Localizer : INotifyPropertyChanged
    {
        public static Localizer Instance { get; } = new();

        public const string Turkish = "Türkçe";
        public const string English = "English";

        private static string _language = Turkish;

        private static readonly Dictionary<string, (string Tr, string En)> Strings = new()
        {
            ["NavDashboard"] = ("Dashboard", "Dashboard"),
            ["NavFavorites"] = ("Favoriler", "Favorites"),
            ["NavPortfolio"] = ("Portföy", "Portfolio"),
            ["NavAlerts"] = ("Alarmlar", "Alerts"),
            ["NavNews"] = ("Haberler", "News"),
            ["NavSettings"] = ("Ayarlar", "Settings"),

            ["MainNavigation"] = ("NAVİGASYON", "NAVIGATION"),
            ["MainToggleSidebar"] = ("Menüyü aç / kapat", "Toggle menu"),
            ["MainConnected"] = ("Bağlı", "Connected"),
            ["MainDisconnected"] = ("Bağlantı Yok", "Disconnected"),
            ["MainNoInternet"] = ("İnternet Yok", "No internet"),
            ["MainFallback"] = ("CoinCap (Yedek)", "CoinCap (fallback)"),

            ["CommonRefresh"] = ("Yenile", "Refresh"),
            ["CommonList"] = ("Liste", "List"),
            ["CommonGrid"] = ("Grid", "Grid"),
            ["CommonAdd"] = ("Ekle", "Add"),
            ["CommonCancel"] = ("İptal", "Cancel"),
            ["CommonCoin"] = ("Coin", "Coin"),
            ["CommonOther"] = ("Diğer", "Other"),
            ["CommonVolume"] = ("HACİM", "VOLUME"),
            ["CommonMarketCap"] = ("MCAP", "MCAP"),
            ["CommonPrice"] = ("FİYAT", "PRICE"),
            ["CommonVolumeShort"] = ("Hacim", "Volume"),
            ["CommonMarketCapShort"] = ("Mcap", "Market cap"),

            ["Period1h"] = ("1s", "1h"),
            ["Period24h"] = ("24s", "24h"),
            ["Period7d"] = ("7g", "7d"),
            ["Period1d"] = ("1G", "1D"),
            ["Period1w"] = ("1H", "1W"),
            ["Period1m"] = ("1Ay", "1M"),
            ["Period3m"] = ("3Ay", "3M"),
            ["Period1y"] = ("1Y", "1Y"),
            ["Period7dShort"] = ("7G", "7D"),
            ["Period30d"] = ("30G", "30D"),

            ["DashboardSubtitle"] = ("Kripto piyasasının canlı görünümü", "Live view of the crypto market"),
            ["DashboardMarketCap"] = ("Piyasa Değeri", "Market Cap"),
            ["DashboardVolume24h"] = ("24s Hacim", "24h Volume"),
            ["DashboardLast24h"] = ("son 24s", "last 24h"),
            ["DashboardLast7d"] = ("son 7g", "last 7d"),
            ["DashboardIncrease"] = ("artış", "increase"),
            ["DashboardFearGreed"] = ("Korku & Açgözlülük", "Fear & Greed"),
            ["DashboardGreed"] = ("Açgözlü", "Greed"),
            ["DashboardChart7d"] = ("7 Günlük Fiyat Grafiği", "7-day price chart"),
            ["DashboardCandlestick"] = ("Mum Grafiği", "Candlestick"),
            ["DashboardMarketDominance"] = ("Piyasa Dominansı", "Market Dominance"),
            ["DashboardTop6"] = ("Top 6 Coin", "Top 6 coins"),
            ["DashboardComparisonSubtitle"] = ("Karşılaştırmalı Fiyat Grafiği", "Comparative price chart"),
            ["DashboardFearGreedIndex"] = ("Korku & Açgözlülük Endeksi", "Fear & Greed Index"),
            ["DashboardTopGainers"] = ("En Çok Yükselenler (24s)", "Top Gainers (24h)"),
            ["DashboardCoinList"] = ("Coin Listesi", "Coin List"),
            ["DashboardTop50"] = ("Top 50 kripto para", "Top 50 cryptocurrencies"),
            ["DashboardLoading"] = ("Veriler yükleniyor...", "Loading data..."),

            ["FavoritesSubtitle"] = ("Takip ettiğiniz coin'ler", "Coins you follow"),
            ["FavoritesEmptyTitle"] = ("Favoriler boş", "No favorites yet"),
            ["FavoritesEmptyHint"] = ("Takip etmek istediğiniz coinleri listeden yıldıza tıklayarak ekleyin.", "Star coins in the list to follow them."),
            ["FavoritesListTitle"] = ("Favori Listesi", "Favorite List"),

            ["PortfolioTitle"] = ("Portföy", "Portfolio"),
            ["PortfolioAddEntry"] = ("+ Giriş Ekle", "+ Add Entry"),
            ["PortfolioTotalValue"] = ("Toplam Değer", "Total Value"),
            ["PortfolioProfitLoss"] = ("Kar/Zarar", "Profit/Loss"),
            ["PortfolioReturnPct"] = ("Getiri %", "Return %"),
            ["PortfolioNewEntry"] = ("Yeni Portföy Girişi", "New Portfolio Entry"),
            ["PortfolioAmount"] = ("Miktar", "Amount"),
            ["PortfolioAvgBuyPrice"] = ("Ort. Alış Fiyatı", "Avg. Buy Price"),
            ["PortfolioAllocation"] = ("Portföy Dağılımı", "Portfolio Allocation"),
            ["PortfolioEmptyTitle"] = ("Henüz portföy girişi eklemediniz", "No portfolio entries yet"),
            ["PortfolioEmptyHint"] = ("Coin listesinden satın aldığınız coinleri ekleyin.", "Add the coins you bought from the coin list."),

            ["AlertsTitle"] = ("Alarmlar", "Alerts"),
            ["AlertsAdd"] = ("+ Alarm Ekle", "+ Add Alert"),
            ["AlertsNew"] = ("Yeni Alarm", "New Alert"),
            ["AlertsCondition"] = ("Koşul", "Condition"),
            ["AlertsTargetPrice"] = ("Hedef Fiyat", "Target Price"),
            ["AlertsEmptyTitle"] = ("Henüz alarm eklemediniz", "No alerts yet"),
            ["AlertsEmptyHint"] = ("Fiyat alarmı eklemek için + butonuna tıklayın", "Click + to add a price alert"),
            ["AlertsActiveHeader"] = ("Aktif Alarmlar ({0})", "Active Alerts ({0})"),
            ["AlertsHistoryHeader"] = ("Geçmiş Alarmlar ({0})", "Alert History ({0})"),
            ["AlertsAbove"] = ("Üstünde", "Above"),
            ["AlertsBelow"] = ("Altında", "Below"),

            ["NewsTitle"] = ("Haberler", "News"),
            ["NewsLastUpdated"] = ("Son güncelleme: {0}", "Last updated: {0}"),
            ["NewsSearchPlaceholder"] = ("Haber başlığı, kaynak veya kategori ara…", "Search news by title, source or category…"),
            ["NewsEmpty"] = ("Aramanıza uygun haber bulunamadı.", "No news matches your search."),
            ["NewsOpen"] = ("Haberi aç", "Open article"),
            ["NewsBack"] = ("Haberler'e dön", "Back to news"),
            ["NewsOpenSource"] = ("Haberin kaynağında aç", "Open source article"),
            ["NewsAll"] = ("Tümü", "All"),
            ["NewsNoSummary"] = ("Bu haber için özet metni bulunmuyor. Kaynağa giderek haberin tamamını okuyabilirsiniz.", "No summary is available for this article. Open the source to read the full story."),

            ["SettingsTitle"] = ("Ayarlar", "Settings"),
            ["SettingsAppearance"] = ("Görünüm", "Appearance"),
            ["SettingsLanguage"] = ("Dil", "Language"),
            ["SettingsCurrency"] = ("Para Birimi", "Currency"),
            ["SettingsApiKeys"] = ("API Anahtarları", "API Keys"),
            ["SettingsCoinGeckoPlaceholder"] = ("CoinGecko API anahtarı", "CoinGecko API key"),
            ["SettingsCryptoComparePlaceholder"] = ("CryptoCompare API anahtarı", "CryptoCompare API key"),
            ["SettingsTestApiKeys"] = ("API Anahtarlarını Test Et", "Test API Keys"),
            ["SettingsConnection"] = ("Bağlantı", "Connection"),
            ["SettingsWebSocket"] = ("WebSocket", "WebSocket"),
            ["SettingsAutomatic"] = ("Otomatik", "Automatic"),
            ["SettingsAbout"] = ("Hakkında", "About"),
            ["SettingsAboutDescription"] = ("Gerçek zamanlı kripto para takip uygulaması", "Real-time cryptocurrency tracking application"),
            ["SettingsSave"] = ("Ayarları Kaydet", "Save Settings"),

            ["ErrorMissingApiKey"] = ("API anahtarı eksik. Ayarlar ekranından girin.", "API key missing. Add it in Settings."),
            ["ErrorCandleChart"] = ("Mum grafiği yüklenemedi: {0}", "Could not load candlestick chart: {0}"),
            ["ErrorComparisonChart"] = ("Karşılaştırma grafiği yüklenemedi: {0}", "Could not load comparison chart: {0}"),
            ["ErrorChart"] = ("Grafik yüklenemedi: {0}", "Could not load chart: {0}"),
            ["ErrorData"] = ("Veri yüklenemedi: {0}", "Could not load data: {0}"),
            ["ErrorCoinGecko"] = ("CoinGecko API hatası: {0}", "CoinGecko API error: {0}"),
            ["ErrorWebSocket"] = ("WebSocket bağlantısı kurulamadı", "Could not connect to WebSocket"),
            ["ErrorInternetLost"] = ("İnternet bağlantısı kesildi", "Internet connection lost"),
            ["ErrorLink"] = ("Haber bağlantısı açılamadı", "Could not open the news link"),
            ["ErrorSelectCoin"] = ("Coin seçin", "Select a coin"),
            ["ErrorEnterAmount"] = ("Miktar girin", "Enter an amount"),
            ["ErrorEnterTarget"] = ("Hedef fiyat girin", "Enter a target price"),

            ["ToastCoinGeckoValid"] = ("CoinGecko API anahtarı geçerli", "CoinGecko API key is valid"),
            ["ToastConnectionRestored"] = ("Bağlantı yenilendi", "Connection restored"),
            ["ToastRefreshed"] = ("Veriler yenilendi", "Data refreshed"),
            ["ToastSettingsSaved"] = ("Ayarlar kaydedildi", "Settings saved"),
            ["ToastPortfolioAdded"] = ("Portföy girişi eklendi", "Portfolio entry added"),
            ["ToastAlertAdded"] = ("Alarm eklendi", "Alert added"),
            ["ToastAlertTriggered"] = ("Alarm tetiklendi: {0} {1} {2}", "Alert triggered: {0} {1} {2}"),

            ["FearGreedDefault"] = ("72 (Açgözlü)", "72 (Greed)"),
            ["FearExtremeFear"] = ("Aşırı Korku", "Extreme Fear"),
            ["FearFear"] = ("Korku", "Fear"),
            ["FearNeutral"] = ("Nötr", "Neutral"),
            ["FearGreed"] = ("Açgözlü", "Greed"),
            ["FearExtremeGreed"] = ("Aşırı Açgözlü", "Extreme Greed"),

            ["CandleOpen"] = ("Açılış", "Open"),
            ["CandleHigh"] = ("Yüksek", "High"),
            ["CandleLow"] = ("Düşük", "Low"),
            ["CandleClose"] = ("Kapanış", "Close"),
            ["GaugeIndex"] = ("Endeks: {0} / 100", "Index: {0} / 100"),
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        public static event Action? LanguageChanged;

        public static string Language => _language;
        public static bool IsTurkish => _language == Turkish;

        public string this[string key] => Translate(key);

        public static void SetLanguage(string? language)
        {
            var normalized = IsEnglish(language) ? English : Turkish;
            var changed = !string.Equals(normalized, _language, StringComparison.Ordinal);
            _language = normalized;

            var culture = CultureInfo.GetCultureInfo(IsEnglish(normalized) ? "en-US" : "tr-TR");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            if (!changed) return;

            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs("Item[]"));
            Instance.PropertyChanged?.Invoke(Instance, new PropertyChangedEventArgs(string.Empty));
            LanguageChanged?.Invoke();
        }

        public static bool IsEnglish(string? language) =>
            string.Equals(language, English, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(language, "en-US", StringComparison.OrdinalIgnoreCase);

        public static string Translate(string key)
        {
            if (Strings.TryGetValue(key, out var entry))
                return IsTurkish ? entry.Tr : entry.En;
            return key;
        }

        public static string Format(string key, params object?[] args) =>
            string.Format(CultureInfo.CurrentCulture, Translate(key), args);

        public static string FearGreedLabel(string? label)
        {
            if (string.IsNullOrWhiteSpace(label)) return string.Empty;

            var key = label.Trim().ToLowerInvariant() switch
            {
                "extreme fear" => "FearExtremeFear",
                "fear" => "FearFear",
                "neutral" => "FearNeutral",
                "greed" => "FearGreed",
                "extreme greed" => "FearExtremeGreed",
                _ => null
            };

            return key is null ? label : Translate(key);
        }
    }
}
