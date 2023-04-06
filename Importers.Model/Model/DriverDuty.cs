using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Runtime.Serialization;

#pragma warning disable CS0649, CS8618, IDE0044, RCS1169

namespace TimetablePlanning.Importers.Model;

[DataContract(IsReference = true)]
public class DriverDuty : IEquatable<DriverDuty>
{
    public DriverDuty(string identity)
    {
        Identity = identity.TextOrException(nameof(identity));
        Parts = new List<TrainPart>();
        Notes = new List<Note>();
    }

    [DataMember(IsRequired = false, Order = 1, Name = "Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    private int _Id;

    public int Id => _Id;

    [DataMember(IsRequired = true, Order = 2)]
    public string Identity { get; }

    [DataMember(IsRequired = true, Order = 3)]
    public ICollection<TrainPart> Parts { get; }

    [DataMember(IsRequired = true, Order = 4)]
    public ICollection<Note> Notes { get; }

    public Schedule Schedule { get; internal set; }

    public bool Equals(DriverDuty? other) => Identity.Equals(other?.Identity, StringComparison.OrdinalIgnoreCase);
    public override bool Equals(object? obj) => obj is DriverDuty other && Equals(other);
    public override int GetHashCode() => Identity.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public override string ToString() =>
        Parts.Count == 0 ? Identity :
        string.Format(CultureInfo.CurrentCulture,
            "{0}: {1} - {2}", Identity, Parts.First().Departure, Parts.Last().Arrival);

    private DriverDuty() { } // Required for deserialization and EF.
}

public static class DriverDutyExtensions
{
    public static Maybe<TrainPart> Add(this DriverDuty duty, TrainPart part)
    {
        duty = duty.ValueOrException(nameof(duty));
        part = part.ValueOrException(nameof(part));
        if (!duty.Parts.Contains(part))
        {
            if (part.IsOverlapping(duty.Parts)) return new Maybe<TrainPart>($"Part {part} overlaps existing parts in driver duty '{duty.Identity}'");
            part.Duty = duty;
            duty.Parts.Add(part);
        }
        return new Maybe<TrainPart>(part);
    }
}
