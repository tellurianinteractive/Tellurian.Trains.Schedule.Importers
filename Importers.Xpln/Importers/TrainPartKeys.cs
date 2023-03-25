namespace TimetablePlanning.Importers.Model.Xpln;

public sealed partial class XplnDataImporter
{    internal record TrainPartKeys(Maybe<StationCall> FromCall, Maybe<StationCall> ToCall, IEnumerable<Message> Messages);
}
