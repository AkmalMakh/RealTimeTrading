using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using TradingBackend.Models;

namespace TradingBackend.Hubs
{
    public class MarketHub : Hub
    {
        private static string Path = "./Data/tradeHistory.json";
        public async Task SubscribeToTicker(string ticker)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticker);
        }

        public async Task UnsubscribeFromTicker(string ticker)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticker);
        }

        public async Task SubmitTrade(TradeRecord trade)
        {
            var list = File.Exists(Path)
                ? JsonSerializer.Deserialize<List<TradeRecord>>(await File.ReadAllTextAsync(Path))!
                : new List<TradeRecord>();

            list.Add(trade);
            await File.WriteAllTextAsync(Path, JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true }));

        }
    }
}