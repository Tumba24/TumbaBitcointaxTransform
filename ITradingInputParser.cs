using System;

namespace Tumba.BitcointaxTransform
{
    public interface ITradingInputParser : IDisposable
    {
        void SetData(string data);
        bool TryReadNext(out bool endOfData, out TradingOutputRecord record, out string errorMessage);
    }
}