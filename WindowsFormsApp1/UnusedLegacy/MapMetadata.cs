namespace CrystalTable.Models
{
    public sealed class MapMetadata
    {
        public MapMetadata(string lotNumber, string waferNumber, string note)
        {
            LotNumber = lotNumber?.Trim() ?? string.Empty;
            WaferNumber = waferNumber?.Trim() ?? string.Empty;
            Note = note?.Trim() ?? string.Empty;
        }

        public string LotNumber { get; }
        public string WaferNumber { get; }
        public string Note { get; }
    }
}

