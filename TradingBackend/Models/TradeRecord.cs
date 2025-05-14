namespace TradingBackend.Models
{
    public class TradeRecord
    {
        public TickerSymbol Ticker { get; set; }
        public TradeSide Side { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime Time { get; set; }
    }
}
