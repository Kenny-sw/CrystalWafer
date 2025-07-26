namespace WaferVision.Core.Services.Interfaces;

public interface IDataImporter
{
    IEnumerable<(double x, double y, double value)> Import(string path);
}
