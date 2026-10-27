using System.Collections.ObjectModel;

namespace Sferix.SPO.Abstractions.Metadata;

/// <summary>Immutable description of one persistent entity.</summary>
/// <remarks>Produced by the source generator; it does not describe tables, relations or model revisions.</remarks>
public sealed class EntityMetadata
{
    /// <summary>Creates the description; the property list is copied.</summary>
    /// <param name="id">The stable entity ID.</param>
    /// <param name="clrName">The namespace-qualified class name.</param>
    /// <param name="properties">The persistent properties.</param>
    public EntityMetadata(EntityId id, string clrName, IEnumerable<PropertyMetadata> properties)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clrName);
        ArgumentNullException.ThrowIfNull(properties);
        Id = id;
        ClrName = clrName;
        Properties = new ReadOnlyCollection<PropertyMetadata>(properties.ToArray());
    }

    /// <summary>The stable entity ID from <see cref="SpoEntityAttribute"/>.</summary>
    public EntityId Id { get; }

    /// <summary>The namespace-qualified class name (nested classes are dot-separated).</summary>
    public string ClrName { get; }

    /// <summary>The persistent properties, ordered by field number.</summary>
    public IReadOnlyList<PropertyMetadata> Properties { get; }
}
