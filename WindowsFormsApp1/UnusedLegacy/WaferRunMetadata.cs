using System;

namespace CrystalTable.Models
{
    public sealed class WaferRunMetadata
    {
        public WaferRunMetadata(string lotNumber, string waferSerial, string operatorName, string note)
        {
            LotNumber = lotNumber?.Trim() ?? string.Empty;
            WaferSerial = waferSerial?.Trim() ?? string.Empty;
            Operator = operatorName?.Trim() ?? string.Empty;
            Note = note?.Trim() ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
        }

        public string LotNumber { get; }
        public string WaferSerial { get; }
        public string Operator { get; }
        public string Note { get; }
        public DateTime CreatedAt { get; }
    }
}

