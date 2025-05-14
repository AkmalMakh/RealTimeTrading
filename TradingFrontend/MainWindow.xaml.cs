using System.Windows;
using TradingFrontend.Services;

namespace TradingFrontend
{
    public partial class MainWindow : Window
    {
        private readonly SignalRService _signalRService;

        public MainWindow()
        {
            InitializeComponent();

            _signalRService = new SignalRService();
            _signalRService.ConnectAsync(); // connect on startup
            if (TickerComboBox != null)
            {
                TickerComboBox.ItemsSource = new[]
                {
                    "ABC", "XYZ", "MIR", "BTC", "ETH", "GOO", "TSL", "QWE", "AAA", "USD"
                };
        
                TickerComboBox.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("⚠️ TickerComboBox is null");
            }
        }

        private void OpenOrderBook_Click(object sender, RoutedEventArgs e)
        {
             if (TickerComboBox.SelectedItem is string ticker)
            {
                var window = new OrderBookWindow(ticker, _signalRService);
                window.Show();
            }
        }

        private void ViewTradeHistory_Click(object sender, RoutedEventArgs e)
        {
            var window = new TradeHistoryWindow();
            window.Show();
        }


    }
}
