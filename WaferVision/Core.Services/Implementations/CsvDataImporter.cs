using WaferVision.Core.Services.Interfaces;

namespace WaferVision.Core.Services.Implementations;

public class CsvDataImporter : IDataImporter
{
    public IEnumerable<(double x, double y, double value)> Import(string path)
    {
        foreach (var line in File.ReadLines(path).Skip(1))
        {
            var parts = line.Split(',');
            if (parts.Length < 3) continue;
            if (double.TryParse(parts[0], out double a) &&
                double.TryParse(parts[1], out double b) &&
                double.TryParse(parts[2], out double v))
            {
                yield return (a, b, v);
            }
        }
    }
}
