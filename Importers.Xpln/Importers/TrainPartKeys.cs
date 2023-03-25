using System.Collections.Generic;
using Tellurian.Trains.Models.Planning;

namespace Tellurian.Trains.Repositories.Xpln;
public sealed partial class XplnDataImporter
{    internal record TrainPartKeys(Maybe<StationCall> FromCall, Maybe<StationCall> ToCall, IEnumerable<Message> Messages);
}
