using System;

namespace Tumba.BitcointaxTransform
{
    public class TradingOutputRecord
    {
        public const string CSV_HEADER = "Date,Action,Source,Symbol,Volume,Price,Currency,Fee,FeeCurrency";

        public DateTimeOffset Date { get; set; }
        public string Source { get; set; }
        public TradingOutputAction Action { get; set; }
        public string Symbol { get; set; }
        public decimal Volume { get; set; }
        public string Currency { get; set; }
        public decimal Price { get; set; }
        public decimal Fee { get; set; }
        public string FeeCurrency { get; set; }

        public string ToCSVLine()
        {
            return string.Format(
                "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                string.Format("{0} {1}", Date.ToString("yyyy-MM-dd HH:mm:ss"), Date.ToString("zzz").Replace(":", "")), // 2014-01-01 13:00:00 -0800
                Action.ToString().ToUpper(),
                Source,
                Symbol,
                Volume,
                Price,
                Currency.ToUpper(),
                Fee,
                FeeCurrency.ToUpper());
        }
    }
}