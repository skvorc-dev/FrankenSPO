using Sferix.SPO.Abstractions.Metadata;

namespace Sferix.SPO.Abstractions.Entities;

/// <summary>The simple base class of P1 entities: shared columns, no inheritance mapping (D-35, D-43).</summary>
/// <remarks>
/// Its field numbers are reserved from 1000001 up; entity field numbers must stay below 1000000.
/// The columns are stored, but nothing fills them yet: the UUIDv7 <see cref="Id"/> arrives with S-03c, the audit
/// timestamps and <see cref="Version"/> with S-04, and soft delete with S-10. Until then they keep their defaults.
/// </remarks>
public abstract class SPOBaseObject
{
    /// <summary>The primary key.</summary>
    [SpoField(1000001)]
    public Guid Id { get; set; }

    /// <summary>When the object was created (UTC).</summary>
    [SpoField(1000002)]
    public DateTime CreatedAt { get; set; }

    /// <summary>When the object was last modified (UTC).</summary>
    [SpoField(1000003)]
    public DateTime ModifiedAt { get; set; }

    /// <summary>Whether the object is soft-deleted.</summary>
    [SpoField(1000004)]
    public bool IsDeleted { get; set; }

    /// <summary>When the object was soft-deleted (UTC), or null.</summary>
    [SpoField(1000005)]
    public DateTime? DeletedAt { get; set; }

    /// <summary>The row version, for conflict detection.</summary>
    [SpoField(1000006)]
    public long Version { get; set; }
}
