using Microsoft.AspNetCore.SignalR.Client;
using System.Diagnostics;
using TradingFrontend.Models;

namespace TradingFrontend.Services
{
    public class SignalRService
    {
        private HubConnection _connection;

        public event Action<string, string>? OrderbookUpdateReceived;

        public async Task ConnectAsync()
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5192/market")
                    .WithAutomaticReconnect()
                    .Build();

                _connection.On<string, string>("ReceiveOrderBookUpdate", (ticker, json) =>
                {
 
                    OrderbookUpdateReceived?.Invoke(ticker, json);
                });

                _connection.Closed += async (error) =>
                {
                    await Task.Delay(2000);
                    await _connection.StartAsync();
                };

                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Aki] SignalR connection failed: {ex.Message}");
            }
        }

        public async Task SubscribeToTicker(string ticker)
        {
            if (_connection.State == HubConnectionState.Connected)
            {
                await _connection.InvokeAsync("SubscribeToTicker", ticker);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[Aki] Cannot subscribe — connection not ready.");
            }
        }

        public async Task SendTradeAsync(TradeRecord trade, CancellationToken ct = default)
        {
            if (trade is null) throw new ArgumentNullException(nameof(trade));

            if (_connection?.State == HubConnectionState.Disconnected)
                await _connection.StartAsync(ct);

            try
            {
                await _connection.InvokeAsync("SubmitTrade", trade, ct);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SendTradeAsync error: {ex.Message}");
            }
        }

    }

}
