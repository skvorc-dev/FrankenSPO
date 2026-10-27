using Sferix.SPO.Abstractions.Entities;
using Sferix.SPO.Abstractions.Metadata;

namespace SpoExample.Module;

/// <summary>A customer with a few typed fields, mapped by the source generator.</summary>
[SpoEntity("3c0a1f7e-5b2d-4e8a-9f61-7d2c4b8e0a15")]
public partial class Customer : SPOBaseObject
{
    [SpoField(1)] public string Name { get; set; } = "";

    [SpoField(2)] public decimal Credit { get; set; }

    [SpoField(3)] public CustomerTier Tier { get; set; }

    /// <summary>When the customer last ordered, in UTC, or null.</summary>
    [SpoField(4)] public DateTime? LastOrderAt { get; set; }
}
