namespace Sferix.SPO.Abstractions.Schema;

/// <summary>The provider-neutral storage type of a column.</summary>
/// <remarks>Enums are stored as <see cref="Int32"/> or <see cref="Int64"/> by their underlying type. A provider maps each value to its own column type.</remarks>
public enum SpoStorageType
{
    /// <summary>A true/false value.</summary>
    Boolean,
    /// <summary>A 32-bit signed integer.</summary>
    Int32,
    /// <summary>A 64-bit signed integer.</summary>
    Int64,
    /// <summary>An exact decimal number.</summary>
    Decimal,
    /// <summary>A double-precision floating-point number.</summary>
    Double,
    /// <summary>Text.</summary>
    String,
    /// <summary>A GUID.</summary>
    Guid,
    /// <summary>A date and time without offset.</summary>
    DateTime,
    /// <summary>A date and time with offset.</summary>
    DateTimeOffset,
    /// <summary>A byte array.</summary>
    Binary,
}
