namespace TimetablePlanning.Importers.Model;

public sealed record Note
{
    public int Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public bool IsDriverNote { get; init; }
    public bool IsStationNote { get; init; }
    public bool IsShuntingNote { get; init; }
    public override string ToString() => Text;
}
