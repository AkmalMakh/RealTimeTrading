using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingFrontend.Models
{
    public enum TradeSide { Buy, Sell}
    public enum TickerSymbol
    {
        ABC, XYZ, MIR, BTC, ETH, GOO, TSL, QWE, AAA, USD
    }
    public class TradeRecord
    {
        public TickerSymbol Ticker {  get; set; }
        public TradeSide Side { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime Time { get; set; }

    }
}
