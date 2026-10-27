namespace Sferix.SPO.Abstractions.Metadata;

/// <summary>Immutable description of one persistent property of an entity.</summary>
/// <remarks>Produced by the source generator. It does not describe columns; for an enum it records the integer type used for storage.</remarks>
public sealed class PropertyMetadata
{
    /// <summary>Creates the description.</summary>
    /// <param name="number">The stable field number (positive).</param>
    /// <param name="clrName">The property name.</param>
    /// <param name="clrType">The property type without the nullable wrapper.</param>
    /// <param name="isNullable">Whether the property accepts null.</param>
    /// <param name="enumUnderlyingType">The underlying integer type when <paramref name="clrType"/> is an enum; otherwise null.</param>
    /// <param name="getValue">Reads the property from an entity instance; null when the metadata was not generated.</param>
    /// <param name="setValue">Writes the property on an entity instance; null when the metadata was not generated.</param>
    public PropertyMetadata(int number, string clrName, Type clrType, bool isNullable, Type? enumUnderlyingType = null,
        Func<object, object?>? getValue = null, Action<object, object?>? setValue = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(number);
        ArgumentException.ThrowIfNullOrWhiteSpace(clrName);
        ArgumentNullException.ThrowIfNull(clrType);
        Number = number;
        ClrName = clrName;
        ClrType = clrType;
        IsNullable = isNullable;
        EnumUnderlyingType = enumUnderlyingType;
        GetValue = getValue;
        SetValue = setValue;
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

    /// <summary>The enum's underlying integer type, or null when the property is not an enum.</summary>
    public Type? EnumUnderlyingType { get; }

    /// <summary>Reads the property from an entity instance without reflection; boxed, null for a null value.</summary>
    /// <remarks>Null on hand-written metadata; the generator always supplies it.</remarks>
    public Func<object, object?>? GetValue { get; }

    /// <summary>Writes the property on an entity instance without reflection; the value must be of the property's type.</summary>
    /// <remarks>Null on hand-written metadata; the generator always supplies it.</remarks>
    public Action<object, object?>? SetValue { get; }
}
