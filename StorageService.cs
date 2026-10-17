using System.Text.Json;

namespace DeveloperPortfolioShowcase;

public sealed class StorageService
{
    private readonly string _folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "DeveloperPortfolioShowcase");
    private string DataPath => Path.Combine(_folder, "portfolio-data.json");

    private readonly JsonSerializerOptions _options = new() { WriteIndented = true };

    public StorageService() => Directory.CreateDirectory(_folder);

    public string DataFolder => _folder;

    public AppData Load()
    {
        try
        {
            if (!File.Exists(DataPath)) return new AppData();
            var json = File.ReadAllText(DataPath);
            return JsonSerializer.Deserialize<AppData>(json, _options) ?? new AppData();
        }
        catch
        {
            return new AppData();
        }
    }

    public void Save(AppData data)
    {
        Directory.CreateDirectory(_folder);
        var temp = DataPath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(data, _options));
        File.Copy(temp, DataPath, true);
        File.Delete(temp);
    }

    public void Export(AppData data, string path) =>
        File.WriteAllText(path, JsonSerializer.Serialize(data, _options));

    public AppData Import(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AppData>(json, _options)
            ?? throw new InvalidDataException("The selected file did not contain valid portfolio data.");
    }
}
