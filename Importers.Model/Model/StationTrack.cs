using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS0649

namespace TimetablePlanning.Importers.Model;

[DataContract(IsReference = true)]
public sealed record StationTrack : IEquatable<StationTrack>
{
    public StationTrack(string number) : this(number, true, true) { }

    public StationTrack(string number, bool isMain, bool isScheduled)
    {
        Number = number;
        IsMain = isMain;
        IsScheduled = isScheduled;
        Calls = new List<StationCall>();
    }

    [DataMember(IsRequired = false, Order = 1, Name = "Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int _Id;

    public int Id => _Id;

    [DataMember(IsRequired = true, Order = 1)]
    public string Number { get; }

    [DataMember(IsRequired = true, Order = 2)]
    public bool IsScheduled { get; init; } = true;

    [DataMember(IsRequired = true, Order = 3)]
    public bool IsMain { get; init; }

    [DataMember(IsRequired = true, Order = 4)]
    public double Length { get; init; }
    [DataMember(IsRequired = true, Order = 5)]
    public string Usage { get; init; } = string.Empty;
    [DataMember(IsRequired = true, Order = 6)]
    public int DisplayOrder { get; init; }

    public Station Station { get; internal set; }

    public ICollection<StationCall> Calls { get; }

    public bool Equals(StationTrack? other) => Number.Equals(other?.Number, StringComparison.OrdinalIgnoreCase) && Station.Equals(other?.Station);
    public override int GetHashCode() => Number.GetHashCode(StringComparison.OrdinalIgnoreCase);
    public override string ToString() => Number;
    public static StationTrack Example { get { return new StationTrack("1") { Station=Station.Example }; } }
    private StationTrack() { } // Only for deserialization.
    public void SetId(int id) => _Id = id;
}

public static class StationTrackExtensions
{
    internal static StationCall Add(this StationTrack me, StationCall call)
    {
        if (call == null) throw new ArgumentNullException(nameof(call));
        if (!me.Calls.Contains(call)) {
            me.Calls.Add(call);
        }
        return call;
    }
}
