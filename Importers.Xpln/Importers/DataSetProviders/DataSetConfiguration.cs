namespace TimetablePlanning.Importers.Xpln.DataSetProviders;
public class DataSetConfiguration
{
    private readonly List<WorksheetConfiguration> _WorksheetConfigurations = new();
    public IEnumerable<WorksheetConfiguration> WorksheetConfigurations => _WorksheetConfigurations;
    public string[] Worksheets => _WorksheetConfigurations.Select(x => x.Name).ToArray();
    public void Add(WorksheetConfiguration configuration)
    {
        if (!ContainsWorksheet(configuration.Name))
            _WorksheetConfigurations.Add(configuration);
    }

    public bool ContainsWorksheet(string? name) => 
        !string.IsNullOrEmpty(name) && 
        _WorksheetConfigurations.Any(wc => wc.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public WorksheetConfiguration? WorksheetConfiguration(string? name) =>
        string.IsNullOrEmpty(name) ? null :
        _WorksheetConfigurations.SingleOrDefault(wc => wc.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

public record WorksheetConfiguration(string Name, int Colums);

