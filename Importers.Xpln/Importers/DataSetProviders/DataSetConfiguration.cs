namespace TimetablePlanning.Importers.Xpln.DataSetProviders;
public record DataSetConfiguration(string Name)
{
    private readonly List<WorksheetConfiguration> _WorksheetConfigurations = [];

    public string[] Worksheets =>
        _WorksheetConfigurations.Select(x => x.WorksheetName).ToArray();

    public void Add(WorksheetConfiguration configuration)
    {
        if (!ContainsWorksheet(configuration.WorksheetName))
            _WorksheetConfigurations.Add(configuration);
    }

    private bool ContainsWorksheet(string? name) =>
        !string.IsNullOrEmpty(name) &&
        _WorksheetConfigurations.Any((Func<WorksheetConfiguration, bool>)(wc => wc.WorksheetName.Equals(name, StringComparison.OrdinalIgnoreCase)));

    public WorksheetConfiguration? WorksheetConfiguration(string? name) =>
        string.IsNullOrEmpty(name) ? null :
        _WorksheetConfigurations.SingleOrDefault((Func<WorksheetConfiguration, bool>)(wc => wc.WorksheetName.Equals(name, StringComparison.OrdinalIgnoreCase)));

}

public record WorksheetConfiguration(string WorksheetName, int MaxReadColumns)
{
    /// <summary>
    /// I a row is repeaded more than this number, reading worksheet will stop. This indicates empty rows.
    /// </summary>
    public int MaxRowRepetitions { get; init; } = 10;
};

