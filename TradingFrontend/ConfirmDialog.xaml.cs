using System;
using System.Windows;
using TradingFrontend.Models;
using TradingFrontend.Services;

namespace TradingFrontend
{

    public partial class ConfirmDialog : Window
    {
        private readonly string _ticker;
        private readonly string _rowText;
        private readonly SignalRService _service;

        public ConfirmDialog(string ticker, string orderRow, SignalRService service)
        {
            InitializeComponent();

            _ticker = ticker;
            _rowText = orderRow.Trim();
            _service = service;

            MessageText.Text = $"{_ticker}: {_rowText}";
        }

        #region Button handlers
        private void Buy_Click(object sender, RoutedEventArgs e) => HandleTrade(TradeSide.Buy);
        private void Sell_Click(object sender, RoutedEventArgs e) => HandleTrade(TradeSide.Sell);
        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();
        #endregion

        private void HandleTrade(TradeSide side)
        {
            if (!TryParseRow(_rowText, out var price, out var qty))
            {
                MessageBox.Show("Cannot parse price / quantity.");
                return;
            }

            var trade = new TradeRecord
            {
                Ticker = Enum.Parse<TickerSymbol>(_ticker),
                Side = side,
                Price = price,
                Quantity = qty,
                Time = DateTime.UtcNow
            };

            MessageBox.Show($"{side} confirmed: {_rowText}");
            
            SendTrade(trade);
            Close();
        }

        private async void SendTrade(TradeRecord trade)
        {
            await _service.SendTradeAsync(trade);
        }

        private static bool TryParseRow(string input, out decimal price, out int qty)
        {
            price = 0; qty = 0;
            var parts = input.Split('|', StringSplitOptions.TrimEntries);
            return parts.Length == 2 &&
                   decimal.TryParse(parts[0], out price) &&
                   int.TryParse(parts[1], out qty);
        }
    }
}