using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using TradingFrontend.Services;

namespace TradingFrontend
{
    public partial class OrderBookWindow : Window
    {
        private readonly string _ticker;
        private readonly SignalRService _service;

        private ObservableCollection<string> Asks { get; } = new();
        private ObservableCollection<string> Bids { get; } = new();

        private string? _selectedAsk;
        private string? _selectedBid;

        private bool _suppressSelection = false;
        public OrderBookWindow(string ticker, SignalRService service)
        {
            InitializeComponent();

            _ticker = ticker;
            _service = service;

            Title = $"Order Book - {_ticker}";
            TickerTitleText.Text = $"{_ticker} Order Book";

            AsksListBox.ItemsSource = Asks;
            BidsListBox.ItemsSource = Bids;

            _service.OrderbookUpdateReceived += OnOrderbookUpdateReceived;
            _ = _service.SubscribeToTicker(_ticker); 
        }

        private void OnOrderbookUpdateReceived(string ticker, string json)
        {
            if (ticker != _ticker) return;

            try
            {
                var doc = JsonDocument.Parse(json);
                var asks = doc.RootElement.GetProperty("Asks");
                var bids = doc.RootElement.GetProperty("Bids");

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _suppressSelection = true;
                    var prevAsk = _selectedAsk;
                    var prevBid = _selectedBid;

                    Asks.Clear();
                    Bids.Clear();

                    foreach (var ask in asks.EnumerateArray())
                        Asks.Add($"{ask.GetProperty("Price").GetDecimal()} | {ask.GetProperty("Quantity").GetInt32()}");

                    foreach (var bid in bids.EnumerateArray())
                        Bids.Add($"{bid.GetProperty("Price").GetDecimal()} | {bid.GetProperty("Quantity").GetInt32()}");

                    AsksListBox.SelectedItem = prevAsk != null && Asks.Contains(prevAsk) ? prevAsk : null;
                    BidsListBox.SelectedItem = prevBid != null && Bids.Contains(prevBid) ? prevBid : null;

                    _suppressSelection = false;
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Order‑book parse error: {ex.Message}");
            }
        }


        private void AsksListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelection) return;

            if (AsksListBox.SelectedItem is string ask)
            {
                _selectedAsk = ask;
                OpenConfirmDialog(ask);
            }
        }

        private void BidsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressSelection) return;
            if (BidsListBox.SelectedItem is string bid)
            {
                _selectedBid = bid;
                OpenConfirmDialog(bid);
            }
        }

        private void OpenConfirmDialog(string row)
        {
            new ConfirmDialog(_ticker, row, _service).ShowDialog();
        }


        protected override void OnClosed(EventArgs e)
        {
            _service.OrderbookUpdateReceived -= OnOrderbookUpdateReceived;
            base.OnClosed(e);
        }
    }
}
