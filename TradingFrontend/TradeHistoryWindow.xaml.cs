using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using TradingFrontend.Models; 


namespace TradingFrontend
{
    public partial class TradeHistoryWindow : Window
    {
        private ObservableCollection<TradeRecord> Trades { get; } = new();

        public TradeHistoryWindow()
        {
            InitializeComponent();
            LoadTrades();
            TradeListView.ItemsSource = Trades;
        }

        private void LoadTrades()
        {
            string path;
            try
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\TradingBackend\Data\tradeHistory.json");
                path = Path.GetFullPath(path);
               
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var trades = JsonSerializer.Deserialize<ObservableCollection<TradeRecord>>(json);
              
                    if (trades != null)
                    {
                        foreach (var trade in trades)
                            Trades.Add(trade);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading trades: {ex.Message}");
            }
        }
    }
}
