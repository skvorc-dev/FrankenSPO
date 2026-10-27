using Sferix.SPO.Abstractions.Resources;

namespace Sferix.SPO.Abstractions.Metadata;

/// <summary>A stable, non-empty identifier for a mapped entity type.</summary>
/// <remarks>This value object is infrastructure only; entity mapping is not yet implemented.</remarks>
public readonly record struct EntityId
{
    public Guid Value { get; }

    public EntityId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException(AbstractionsMessages.EntityIdEmpty, nameof(value));
        Value = value;
    }

    public override string ToString() => Value.ToString("D");
}
