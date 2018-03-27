using System;
using System.Globalization;
using System.IO;

namespace Tumba.BitcointaxTransform
{
    public class UpholdTradingParser : ITradingInputParser
    {
        private StringReader m_reader;

        public int NextLine { get; private set; }

        public void Dispose()
        {
            if (m_reader != null) 
            {
                m_reader.Dispose();
            }   
        }

        public void SetData(string data)
        {
            if (m_reader != null)
            {
                m_reader.Dispose();
            }

            m_reader = new StringReader(data);
            m_reader.ReadLine(); // Skip header

            NextLine = 1;
        }

        public bool TryCreateTradingOutputRecord(string[] parts, out TradingOutputRecord record, out string errorMessage)
        {
            record = null;
            errorMessage = null;
            
            DateTimeOffset date;
            if (!DateTimeOffset.TryParse(parts[0], null, DateTimeStyles.AssumeUniversal, out date))
            {
                errorMessage = string.Format("Invalid date on line {0}! Found: {1}", NextLine, parts[0]);
                return false;
            }

            string upholdType = parts[2];
            string originCurrency = parts[7];
            string destinationCurrency = parts[10];

            if (!(upholdType.Equals("transfer", StringComparison.InvariantCultureIgnoreCase) ||
                    upholdType.Equals("deposit", StringComparison.InvariantCultureIgnoreCase)) || 
                originCurrency == destinationCurrency)
            {
                return true;
            }

            decimal volume;
            if (!decimal.TryParse(parts[11], out volume))
            {
                errorMessage = string.Format("Invalid volume on line {0}! Found: {1}", NextLine, parts[11]);
                return false;
            }

            decimal rate;
            if (!decimal.TryParse(parts[6], out rate))
            {
                errorMessage = string.Format("Invalid rate on line {0}! Found: {1}", NextLine, parts[6]);
                return false;
            }

            decimal originAmount;
            if (!decimal.TryParse(parts[8], out originAmount))
            {
                errorMessage = string.Format("Invalid origin amount on line {0}! Found: {1}", NextLine, parts[8]);
                return false;
            }

            decimal valueInUSD;
            if (!decimal.TryParse(parts[3], out valueInUSD))
            {
                errorMessage = string.Format("Invalid value in usd on line {0}! Found: {1}", NextLine, parts[3]);
                return false;
            }

            record = new TradingOutputRecord();

            record.Date = date;
            record.Source = "Uphold";
            record.Action = TradingOutputAction.BUY;
            record.Symbol = destinationCurrency;
            record.Volume = volume;
            record.Currency = originCurrency;
            
            if (destinationCurrency.Equals("USD", StringComparison.InvariantCultureIgnoreCase))
            {
                record.Action = TradingOutputAction.SELL;
                record.Symbol = originCurrency;
                record.Volume = originAmount;
                record.Currency = destinationCurrency;

                decimal originAmountInDestiantionCurrency = originAmount * rate;
                decimal feeInDestiantionCurrency = originAmountInDestiantionCurrency - volume;
                
                record.Fee = feeInDestiantionCurrency;
                record.FeeCurrency = destinationCurrency;
            }
            else
            {
                string pair = parts[5];
                if (pair.StartsWith(originCurrency) && pair.EndsWith(destinationCurrency))
                {
                    rate = 1 / rate;
                }

                decimal originAmountInDestiantionCurrency = originAmount / rate;
                decimal feeInDestiantionCurrency = originAmountInDestiantionCurrency - volume;

                record.Fee = feeInDestiantionCurrency * rate;
                record.FeeCurrency = originCurrency;
            }

            record.Price = rate;
            
            return true;
        }

        public bool TryReadNext(out bool endOfData, out TradingOutputRecord record, out string errorMessage)
        {
            endOfData = false;
            record = null;
            errorMessage = null;

            try
            {
                string line = m_reader.ReadLine();
                if (line == null)
                {
                    endOfData = true; // No more lines to read.
                    return true;
                }

                line = line.Trim();
                if (line.Length < 1)
                {
                    return true; // Empty line.
                }

                string[] parts;
                if (!TryReadParts(line, out parts, out errorMessage))
                {
                    return false;
                }

                return TryCreateTradingOutputRecord(parts, out record, out errorMessage);
            }
            finally
            {
                NextLine++;
            }
        }

        public bool TryReadParts(string line, out string[] parts, out string errorMessage)
        {
            parts = line.Split(',');
            if (parts.Length !=  13)
            {
                errorMessage = string.Format(
                    "Invalid line {0}!  Expected {1} parts but found {2} : {3}",
                    NextLine,
                    13,
                    parts.Length,
                    line);

                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}