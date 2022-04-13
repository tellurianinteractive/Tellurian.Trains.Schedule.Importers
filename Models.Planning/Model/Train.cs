using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS0649, IDE0044, RCS1169

namespace Tellurian.Trains.Models.Planning
{
    [DataContract(IsReference = true)]
    public class Train : IEquatable<Train>
    {
        public Train(string number)
        {
            Number = !string.IsNullOrWhiteSpace(number) ? number : throw new ArgumentNullException(nameof(number));
            Calls = new List<StationCall>();
            ExtenalId = string.Empty;
        }

        [DataMember(IsRequired = false, Order = 1, Name = "Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        private int _Id;

        public int Id => _Id;

        public Train(string number, string externalId) : this(number)
        {
            ExtenalId = !string.IsNullOrWhiteSpace(externalId) ? externalId : throw new ArgumentNullException(nameof(externalId));
        }

        [DataMember(IsRequired = true, Order = 1)]
        public string Number { get; }

        [DataMember(IsRequired = true, Order = 2)]
        public string Category { get; set; } = string.Empty;

        [DataMember(IsRequired = true, Order = 3)]
        public string ExtenalId { get; }

        public IList<StationCall> Calls { get; }

        public StationCall this[int index] => Calls[index];
        internal IEnumerable<StationTrack> Tracks => Calls.OrderBy(c => c.Arrival.Value).Select(c => c.Track).Distinct();
        public Layout Layout => Calls[0].Station.Layout;
        public Timetable Timetable { get; internal set; }
        public TrainPart AsTrainPart => this.AsTrainPart(0, Calls.Count - 1);

        public bool Equals(Train? other) =>
            other is not null && Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase) &&
            ExtenalId.Equals(other?.ExtenalId, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => obj is Train other && Equals(other);
        public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase) ^ ExtenalId.GetHashCode(StringComparison.OrdinalIgnoreCase);
        public override string ToString() => string.Format(CultureInfo.CurrentCulture, "{0} {1}", Category, Number);

        private Train() { } // For deserialization.

 
    }

    public static class TrainExtensions
    {
        public static StationCall Add(this Train train, StationCall call)
        {
            train = train.ValueOrException(nameof(train));
            call = call.ValueOrException(nameof(call));
            if (!train.Calls.Contains(call))
            {
                call.Train = train;
                train.Calls.Add(call);
            }
            return call;
        }

        public static Train WithFixedFirstAndLastCall(this Train train)
        {
            train.Calls.First().IsArrival = false;
            train.Calls.Last().IsDeparture = false;
            return train;
        }
       public static Train WithFixedSingleCallTrain(this Train train)
        {
            if (train.Calls.Count == 1)
            {
                var departure = train.Calls[0];
                departure.Track.Calls.Remove(departure);
                var arrival = new StationCall(departure.Track, departure.Arrival, departure.Arrival);
                departure = new StationCall(departure.Track,  departure.Departure, departure.Departure);
                train.Calls.Clear();
                train.Add(arrival);
                train.Add(departure);
            }
            return train;
        }

    }
}
