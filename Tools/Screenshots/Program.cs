using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaAppCrypto;
using AvaloniaAppCrypto.Interfaces;
using AvaloniaAppCrypto.Localization;
using AvaloniaAppCrypto.Models;
using AvaloniaAppCrypto.ViewModels;
using AvaloniaAppCrypto.Views;

namespace Screenshots
{
    /// <summary>
    /// Captures the real Avalonia views with representative English demo data.
    /// The Python wrapper in scripts/make_screenshots.py adds the macOS window
    /// frame and writes the final SVG files to docs/img/.
    /// </summary>
    internal static class Program
    {
        private const int Width = 1440;
        private const int Height = 900;

        public static int Main(string[] args)
        {
            var output = args.Length > 0 ? args[0] : "screenshots";
            Directory.CreateDirectory(output);

            AppBuilder.Configure<App>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
                .UseSkia()
                .WithInterFont()
                .SetupWithoutStarting();

            Localizer.SetLanguage(Localizer.English);

            var viewModel = new MainWindowViewModel(
                new DemoCoinGeckoService(),
                new DemoCryptoCompareService(),
                new DemoBinanceService(),
                new DemoCoinCapService(),
                new DemoConnectivityService(),
                new DemoToastService(),
                new DemoFavoriteService(),
                new DemoAlertService(),
                new DemoPortfolioService(),
                new DemoSettingsService());

            viewModel.InitializationTask.GetAwaiter().GetResult();
            Pump();

            var window = new MainWindow
            {
                DataContext = viewModel,
                Width = Width,
                Height = Height
            };
            window.Show();
            Pump(20);

            foreach (var coin in viewModel.Coins)
                coin.ImageSource = DemoImages.Badge(coin.Symbol, DemoImages.ColorFor(coin.Symbol));
            foreach (var article in viewModel.NewsArticles)
                article.CoverImage = DemoImages.Cover(article.Categories);

            Capture(window, vm: viewModel, output, "dashboard");
            viewModel.NavigateToCommand.Execute("Portfolio");
            Pump(20);
            Capture(window, vm: viewModel, output, "portfolio");
            viewModel.NavigateToCommand.Execute("News");
            Pump(20);
            Capture(window, vm: viewModel, output, "news");

            return 0;
        }

        private static void Capture(MainWindow window, MainWindowViewModel vm, string output, string name)
        {
            Dispatcher.UIThread.RunJobs();
            var frame = window.CaptureRenderedFrame();
            if (frame is null)
                throw new InvalidOperationException($"Could not capture the {name} frame.");

            var path = Path.Combine(output, name + ".png");
            frame.Save(path, new PngBitmapEncoderOptions());
            Console.WriteLine($"{path} ({Width}x{Height})");
        }

        private static void Pump(int rounds = 10)
        {
            for (var i = 0; i < rounds; i++)
            {
                Dispatcher.UIThread.RunJobs();
                Thread.Sleep(15);
            }
        }
    }

    internal static class DemoImages
    {
        private static readonly Dictionary<string, string> Colors = new(StringComparer.OrdinalIgnoreCase)
        {
            ["btc"] = "#F7931A",
            ["eth"] = "#627EEA",
            ["sol"] = "#14F195",
            ["bnb"] = "#F0B90B",
            ["xrp"] = "#23292F",
            ["ada"] = "#0033AD",
            ["doge"] = "#C2A633",
            ["avax"] = "#E84142",
            ["link"] = "#2A5ADA",
            ["dot"] = "#E6007A",
            ["ltc"] = "#345D9D",
            ["ton"] = "#0098EA",
            ["shib"] = "#E42D04",
            ["uni"] = "#FF007A",
            ["atom"] = "#2E3148"
        };

        public static string ColorFor(string symbol) =>
            Colors.TryGetValue(symbol, out var color) ? color : "#3B82F6";

        public static IImage Badge(string symbol, string color)
        {
            var bitmap = new RenderTargetBitmap(new PixelSize(64, 64), new Vector(96, 96));
            using (var context = bitmap.CreateDrawingContext())
            {
                var brush = new SolidColorBrush(Color.Parse(color));
                context.DrawEllipse(brush, null, new Point(32, 32), 30, 30);
                var text = new FormattedText(
                    symbol.ToUpperInvariant(),
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter", FontStyle.Normal, FontWeight.Bold),
                    20,
                    Brushes.White);
                context.DrawText(text, new Point(32 - text.Width / 2, 32 - text.Height / 2));
            }
            return bitmap;
        }

        public static IImage Cover(string category)
        {
            var palettes = new Dictionary<string, (string Start, string End, string Label)>
            {

                ["Market"] = ("#16283F", "#0E3B4A", "MARKET"),
                ["ETH"] = ("#1B2350", "#3A2B6B", "ETH"),
                ["Altcoin"] = ("#1D2A24", "#14504A", "ALTCOIN"),
                ["BTC"] = ("#2D2111", "#4A2E08", "BTC"),
                ["DeFi"] = ("#1B2C3A", "#1B4A63", "DEFI")
            };
            var palette = palettes.TryGetValue(category, out var found)
                ? found
                : (Start: "#182033", End: "#26324A", Label: category.ToUpperInvariant());

            var bitmap = new RenderTargetBitmap(new PixelSize(640, 360), new Vector(96, 96));
            using (var context = bitmap.CreateDrawingContext())
            {
                var brush = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(Color.Parse(palette.Start), 0),
                        new GradientStop(Color.Parse(palette.End), 1)
                    }
                };
                context.DrawRectangle(brush, null, new Rect(0, 0, 640, 360));
                context.DrawEllipse(new SolidColorBrush(Color.FromArgb(28, 255, 255, 255)), null,
                    new Point(560, 40), 150, 150);
                context.DrawEllipse(new SolidColorBrush(Color.FromArgb(20, 255, 255, 255)), null,
                    new Point(80, 330), 190, 190);

                var text = new FormattedText(
                    palette.Label,
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter", FontStyle.Normal, FontWeight.Bold),
                    58,
                    new SolidColorBrush(Color.FromArgb(205, 255, 255, 255)));
                context.DrawText(text, new Point(42, 240));
            }
            return bitmap;
        }
    }

    internal sealed class DemoCoinGeckoService : ICoinGeckoService
    {
        public Task<List<Coin>> GetTopCoinsAsync(int perPage = 100, CancellationToken ct = default) =>
            Task.FromResult(DemoData.Coins());

        public Task<Coin?> GetCoinDetailAsync(string coinId, CancellationToken ct = default) =>
            Task.FromResult<Coin?>(null);

        public Task<Dictionary<string, Dictionary<string, decimal>>> GetSimplePriceAsync(
            List<string> coinIds, CancellationToken ct = default) =>
            Task.FromResult(new Dictionary<string, Dictionary<string, decimal>>());

        public Task<List<Dictionary<string, object>>> GetMarketChartAsync(
            string coinId, int days = 7, CancellationToken ct = default)
        {
            var start = coinId == "ethereum" ? 3060.0 : 91800.0;
            var end = coinId == "ethereum" ? 3240.0 : 96420.0;
            var count = Math.Clamp(days * 24, 48, 240);
            return Task.FromResult(DemoData.Series(start, end, count));
        }

        public Task<List<OhlcPoint>> GetOhlcAsync(string coinId, int days = 7, CancellationToken ct = default)
        {
            return Task.FromResult(DemoData.Candles(30));
        }

        public Task<List<Coin>> SearchCoinsAsync(string query, CancellationToken ct = default) =>
            Task.FromResult(DemoData.Coins());

        public Task<Dictionary<string, object>> GetGlobalDataAsync(CancellationToken ct = default) =>
            Task.FromResult(new Dictionary<string, object>
            {
                ["total_market_cap"] = 3_420_000_000_000m,
                ["total_volume"] = 128_400_000_000m,
                ["btc_dominance"] = 56.2m,
                ["market_cap_change_24h"] = 1.8m
            });

        public Task<(int value, string label)> GetFearGreedIndexAsync(CancellationToken ct = default) =>
            Task.FromResult((72, "Greed"));
    }

    internal sealed class DemoCryptoCompareService : ICryptoCompareService
    {
        public Task<List<NewsArticle>> GetLatestNewsAsync(string categories = "", CancellationToken ct = default) =>
            Task.FromResult(DemoData.News());
    }

    internal sealed class DemoBinanceService : IBinanceWsService
    {
        public event Action<string, decimal, decimal>? PriceUpdated;
        public bool IsConnected => true;
        public Task ConnectAsync()
        {
            PriceUpdated?.Invoke(string.Empty, 0, 0);
            return Task.CompletedTask;
        }
        public Task DisconnectAsync() => Task.CompletedTask;
    }

    internal sealed class DemoCoinCapService : ICoinCapWsService
    {
        public event Action<string, decimal>? PriceUpdated;
        public bool IsConnected => true;
        public Task ConnectAsync(List<string> assets)
        {
            PriceUpdated?.Invoke(string.Empty, 0);
            return Task.CompletedTask;
        }
        public Task DisconnectAsync() => Task.CompletedTask;
    }

    internal sealed class DemoConnectivityService : IConnectivityService
    {
        public bool IsOnline => true;
        public event Action<bool>? ConnectivityChanged;
        public Task CheckConnectivityAsync()
        {
            ConnectivityChanged?.Invoke(true);
            return Task.CompletedTask;
        }
    }

    internal sealed class DemoSettingsService : ISettingsService
    {
        private readonly AppSettings _settings = new()
        {
            Language = Localizer.English,
            CoinGeckoApiKey = "demo",
            CryptoCompareApiKey = "demo",
            RefreshIntervalSeconds = 0
        };

        public Task<AppSettings> GetSettingsAsync() => Task.FromResult(_settings);
        public Task SaveSettingsAsync(AppSettings settings) => Task.CompletedTask;
    }

    internal sealed class DemoFavoriteService : IFavoriteService
    {
        private readonly List<string> _favorites = new() { "bitcoin", "ethereum", "solana" };

        public Task<List<string>> GetFavoritesAsync() => Task.FromResult(new List<string>(_favorites));
        public Task<bool> IsFavoriteAsync(string coinId) => Task.FromResult(_favorites.Contains(coinId));
        public Task AddFavoriteAsync(string coinId)
        {
            if (!_favorites.Contains(coinId)) _favorites.Add(coinId);
            return Task.CompletedTask;
        }
        public Task RemoveFavoriteAsync(string coinId)
        {
            _favorites.Remove(coinId);
            return Task.CompletedTask;
        }
        public Task SaveFavoritesAsync(List<string> favorites)
        {
            _favorites.Clear();
            _favorites.AddRange(favorites);
            return Task.CompletedTask;
        }
    }

    internal sealed class DemoAlertService : IAlertService
    {
        private readonly List<PriceAlert> _alerts = new()
        {
            new PriceAlert
            {
                CoinId = "bitcoin", CoinName = "Bitcoin", CoinSymbol = "BTC",
                TargetPrice = 105_000m, Condition = "Above", IsActive = true,
                DateCreated = DateTime.Today.AddDays(-2), CurrentPriceAtCreation = 91_000m
            },
            new PriceAlert
            {
                CoinId = "ethereum", CoinName = "Ethereum", CoinSymbol = "ETH",
                TargetPrice = 2_900m, Condition = "Below", IsActive = true,
                DateCreated = DateTime.Today.AddDays(-5), CurrentPriceAtCreation = 3_350m
            },
            new PriceAlert
            {
                CoinId = "solana", CoinName = "Solana", CoinSymbol = "SOL",
                TargetPrice = 150m, Condition = "Above", IsActive = false,
                DateCreated = DateTime.Today.AddDays(-9), CurrentPriceAtCreation = 138m
            }
        };

        public Task<List<PriceAlert>> GetAlertsAsync() => Task.FromResult(_alerts.ToList());
        public Task AddAlertAsync(PriceAlert alert)
        {
            _alerts.Add(alert);
            return Task.CompletedTask;
        }
        public Task UpdateAlertAsync(PriceAlert alert) => Task.CompletedTask;
        public Task RemoveAlertAsync(string alertId)
        {
            _alerts.RemoveAll(a => a.Id == alertId);
            return Task.CompletedTask;
        }
        public Task<List<PriceAlert>> CheckAlertsAsync(Dictionary<string, decimal> prices) =>
            Task.FromResult(new List<PriceAlert>());
        public Task ClearTriggeredAsync()
        {
            _alerts.RemoveAll(a => !a.IsActive);
            return Task.CompletedTask;
        }
    }

    internal sealed class DemoPortfolioService : IPortfolioService
    {
        private readonly List<PortfolioEntry> _entries = DemoData.Portfolio();

        public Task<List<PortfolioEntry>> GetPortfolioAsync() => Task.FromResult(_entries.ToList());
        public Task AddEntryAsync(PortfolioEntry entry)
        {
            _entries.Add(entry);
            return Task.CompletedTask;
        }
        public Task UpdateEntryAsync(PortfolioEntry entry) => Task.CompletedTask;
        public Task RemoveEntryAsync(string entryId)
        {
            _entries.RemoveAll(e => e.Id == entryId);
            return Task.CompletedTask;
        }
        public Task ClearAllAsync()
        {
            _entries.Clear();
            return Task.CompletedTask;
        }
    }

    internal sealed class DemoToastService : IToastService
    {
        public void Show(string message, ToastType type = ToastType.Info, int durationMs = 4000) { }
    }

    internal static class DemoData
    {
        public static List<Coin> Coins()
        {
            var values = new (string Id, string Symbol, string Name, decimal Price, long Cap, decimal Volume, double H1, double H24, double D7)[]
            {
                ("bitcoin", "btc", "Bitcoin", 96_420.35m, 1_910_000_000_000, 42_800_000_000, 0.42, 2.34, 8.12),
                ("ethereum", "eth", "Ethereum", 3_240.50m, 390_000_000_000, 18_600_000_000, 0.18, 1.65, 5.42),
                ("tether", "usdt", "Tether", 1.00m, 120_000_000_000, 52_000_000_000, 0.01, -0.02, 0.03),
                ("binancecoin", "bnb", "BNB", 712.40m, 103_000_000_000, 2_100_000_000, -0.22, 0.84, 3.15),
                ("solana", "sol", "Solana", 198.42m, 94_000_000_000, 5_400_000_000, 1.12, 4.28, 12.64),
                ("ripple", "xrp", "XRP", 2.34m, 133_000_000_000, 3_900_000_000, 0.08, -1.42, 2.05),
                ("usd-coin", "usdc", "USDC", 1.00m, 61_000_000_000, 8_700_000_000, 0.00, 0.01, -0.01),
                ("cardano", "ada", "Cardano", 0.92m, 32_000_000_000, 1_200_000_000, -0.35, -2.18, -4.32),
                ("dogecoin", "doge", "Dogecoin", 0.28m, 41_000_000_000, 2_800_000_000, 0.64, 3.12, 9.48),
                ("avalanche-2", "avax", "Avalanche", 42.18m, 17_000_000_000, 720_000_000, -0.48, -1.86, 1.24),
                ("chainlink", "link", "Chainlink", 24.62m, 15_000_000_000, 890_000_000, 0.92, 2.74, 7.36),
                ("polkadot", "dot", "Polkadot", 7.84m, 11_000_000_000, 410_000_000, -0.18, 0.42, -2.12),
                ("litecoin", "ltc", "Litecoin", 118.40m, 8_900_000_000, 680_000_000, 0.36, 1.18, 4.02),
                ("toncoin", "ton", "Toncoin", 5.62m, 14_000_000_000, 320_000_000, -0.72, -2.84, -5.16),
                ("shiba-inu", "shib", "Shiba Inu", 0.000024m, 14_000_000_000, 820_000_000, 0.24, 1.96, 6.84),
                ("uniswap", "uni", "Uniswap", 12.86m, 7_700_000_000, 260_000_000, 0.58, 2.22, 5.74),
                ("cosmos", "atom", "Cosmos Hub", 8.24m, 3_200_000_000, 190_000_000, -0.28, 0.74, -1.36),
                ("near", "near", "NEAR Protocol", 5.18m, 6_100_000_000, 380_000_000, 0.82, 3.64, 10.28)
            };

            var ranks = 1;
            return values.Select(v => new Coin
            {
                Id = v.Id,
                Symbol = v.Symbol,
                Name = v.Name,
                CurrentPrice = v.Price,
                MarketCap = v.Cap,
                MarketCapRank = ranks++,
                TotalVolume = v.Volume,
                PriceChangePercentage1H = v.H1,
                PriceChangePercentage24H = v.H24,
                PriceChangePercentage7D = v.D7,
                High24H = v.Price * 1.035m,
                Low24H = v.Price * 0.972m,
                CirculatingSupply = v.Price > 0 ? v.Cap / v.Price : 0,
                Ath = v.Price * 1.35m,
                AthDate = "2025-11-06"
            }).ToList();
        }

        public static List<Dictionary<string, object>> Series(double start, double end, int count)
        {
            var list = new List<Dictionary<string, object>>(count);
            var drift = (end - start) / Math.Max(count - 1, 1);
            var amplitude = Math.Abs(end - start) * 0.028 + 40;
            for (var i = 0; i < count; i++)
            {
                var wave = Math.Sin(i / 5.4) * amplitude
                           + Math.Cos(i / 11.7) * amplitude * 0.55
                           + Math.Sin(i / 2.1) * amplitude * 0.18;
                var value = start + drift * i + wave;
                list.Add(new Dictionary<string, object>
                {
                    ["timestamp"] = DateTimeOffset.UtcNow.AddHours(i - count).ToUnixTimeMilliseconds(),
                    ["price"] = (decimal)Math.Max(value, start * 0.9)
                });
            }
            return list;
        }

        public static List<OhlcPoint> Candles(int count)
        {
            var list = new List<OhlcPoint>(count);
            var value = 91_800.0;
            for (var i = 0; i < count; i++)
            {
                var open = value;
                var move = Math.Sin(i / 2.3) * 640 + Math.Cos(i / 5.1) * 420 + 130;
                var close = Math.Max(open + move, 70_000);
                var high = Math.Max(open, close) + Math.Abs(Math.Sin(i) * 380) + 120;
                var low = Math.Min(open, close) - Math.Abs(Math.Cos(i) * 360) - 100;
                list.Add(new OhlcPoint
                {
                    Time = DateTime.Today.AddDays(-count + i).AddHours(12),
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close
                });
                value = close;
            }
            return list;
        }

        public static List<PortfolioEntry> Portfolio() => new()
        {
            new PortfolioEntry { CoinId = "bitcoin", CoinSymbol = "BTC", CoinName = "Bitcoin", Amount = 0.42m, AverageBuyPrice = 62_150m, DateAdded = DateTime.Today.AddMonths(-8) },
            new PortfolioEntry { CoinId = "ethereum", CoinSymbol = "ETH", CoinName = "Ethereum", Amount = 3.5m, AverageBuyPrice = 2_450m, DateAdded = DateTime.Today.AddMonths(-6) },
            new PortfolioEntry { CoinId = "solana", CoinSymbol = "SOL", CoinName = "Solana", Amount = 40m, AverageBuyPrice = 142.30m, DateAdded = DateTime.Today.AddMonths(-3) }
        };

        public static List<NewsArticle> News() => new()
        {
            Article("Bitcoin holds above $96,000 as spot ETF inflows accelerate",
                "Market", "CoinDesk", 1,
                "Bitcoin traded above $96,000 on Monday as spot ETF products recorded a third straight week of net inflows. Analysts pointed to steady institutional demand and a tightening supply picture ahead of the next halving cycle."),
            Article("Ethereum staking deposits climb to a new high",
                "ETH", "Cointelegraph", 3,
                "The amount of ether locked in staking contracts reached a fresh record this week, with validators now securing more than a quarter of the circulating supply."),
            Article("Solana ecosystem adds three new DeFi protocols",
                "Altcoin", "Decrypt", 5,
                "Developers shipped a new lending market, a perpetuals exchange and a yield aggregator on Solana, extending a busy quarter for the network's DeFi ecosystem."),
            Article("Regulators outline a common framework for stablecoin reserves",
                "Market", "The Block", 8,
                "A group of financial regulators published draft guidance on stablecoin reserve disclosure, setting a six-month consultation window for issuers and exchanges."),
            Article("Chainlink expands cross-chain data feeds to five more networks",
                "Altcoin", "CryptoSlate", 12,
                "Chainlink said its data feeds are now live on five additional networks, widening the reach of its oracle infrastructure for DeFi applications."),
            Article("Crypto market weekly recap: majors steady, altcoins outperform",
                "Market", "CoinDesk", 26,
                "Major tokens ended the week little changed while a basket of large-cap altcoins gained more than 6%, according to market data.")
        };

        private static NewsArticle Article(string title, string categories, string source, int hoursAgo, string body) =>
            new()
            {
                Title = title,
                Categories = categories,
                Source = source,
                Url = "https://example.com/news",
                PublishedOn = DateTime.UtcNow.AddHours(-hoursAgo),
                Body = body
            };
    }
}
