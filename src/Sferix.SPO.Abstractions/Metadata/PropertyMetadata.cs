namespace Sferix.SPO.Abstractions.Metadata;

/// <summary>Immutable description of one persistent property of an entity.</summary>
/// <remarks>Produced by the source generator. It does not describe columns or storage types.</remarks>
public sealed class PropertyMetadata
{
    /// <summary>Creates the description.</summary>
    /// <param name="number">The stable field number (positive).</param>
    /// <param name="clrName">The property name.</param>
    /// <param name="clrType">The property type without the nullable wrapper.</param>
    /// <param name="isNullable">Whether the property accepts null.</param>
    public PropertyMetadata(int number, string clrName, Type clrType, bool isNullable)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(clrName);
        ArgumentNullException.ThrowIfNull(clrType);
        Number = number;
        ClrName = clrName;
        ClrType = clrType;
        IsNullable = isNullable;
    }

    /// <summary>The stable field number from <see cref="SpoFieldAttribute"/>.</summary>
    public int Number { get; }

    /// <summary>The property name.</summary>
    public string ClrName { get; }

    /// <summary>The property type; <c>int?</c> is reported as <see cref="int"/> with <see cref="IsNullable"/> true.</summary>
    /// <remarks>An enum property reports the enum type itself, not its underlying integer type.</remarks>
    public Type ClrType { get; }

    /// <summary>Whether the property accepts null (nullable value type or annotated reference type).</summary>
    public bool IsNullable { get; }
}
