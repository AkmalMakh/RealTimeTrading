using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Timers;
using TradingBackend.Hubs;
using TradingBackend.Models;


namespace TradingBackend.Services
{
    public class OrderbookSimulator : IHostedService, IDisposable
    {
        private readonly ILogger<OrderbookSimulator> _logger;
        private readonly IHubContext<MarketHub> _hubContext;
        private readonly Random _rand = new();

        private readonly List<TickerSymbol> _tickers = Enum.GetValues<TickerSymbol>().ToList();
        private readonly List<TradeRecord> _tradeHistory = new();

        private System.Timers.Timer _timer;
        private static string TradeFilePath = "./Data/trades.json";

        public OrderbookSimulator(IHubContext<MarketHub> hubContext, ILogger<OrderbookSimulator> logger)
        {
            _hubContext = hubContext;
            _logger = logger;

            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += BroadcastUpdates;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (File.Exists(TradeFilePath))
            {
                var json = File.ReadAllText(TradeFilePath);
                var trades = JsonSerializer.Deserialize<List<TradeRecord>>(json);
                if (trades is not null)
                    _tradeHistory.AddRange(trades);
            }

            _timer.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private async void BroadcastUpdates(object? sender, ElapsedEventArgs e)
        {
            foreach (var ticker in _tickers)
            {
                var orderbook = GenerateFakeOrderbook();
                var json = JsonSerializer.Serialize(orderbook);

                await _hubContext.Clients.Group(ticker.ToString())
                    .SendAsync("ReceiveOrderBookUpdate", ticker.ToString(), json);

                var trade = new TradeRecord
                {
                    Ticker = ticker,
                    Side = _rand.Next(2) == 0 ? TradeSide.Buy : TradeSide.Sell,
                    Price = _rand.Next(95, 106),
                    Quantity = _rand.Next(5, 50),
                    Time = DateTime.UtcNow
                };

                _tradeHistory.Add(trade);

                Directory.CreateDirectory(Path.GetDirectoryName(TradeFilePath)!);

                File.WriteAllText(TradeFilePath, JsonSerializer.Serialize(_tradeHistory));
            }
        }

        private object GenerateFakeOrderbook()
        {
            var asks = Enumerable.Range(1, 10)
                .Select(i => new { Price = 100 + i, Quantity = _rand.Next(5, 50) })
                .Reverse()
                .ToList();

            var bids = Enumerable.Range(1, 10)
                .Select(i => new { Price = 100 - i, Quantity = _rand.Next(5, 50) })
                .ToList();

            return new { Asks = asks, Bids = bids };
        }
    }
}
