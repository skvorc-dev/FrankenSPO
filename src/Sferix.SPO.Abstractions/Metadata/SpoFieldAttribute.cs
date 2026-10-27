namespace Sferix.SPO.Abstractions.Metadata;

/// <summary>Marks a property as persistent and gives it a stable field number.</summary>
/// <remarks>
/// The number is positive, unique within the entity and never reused, like a protobuf field number.
/// The source generator validates it; this attribute does not.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SpoFieldAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="number">The stable field number.</param>
    public SpoFieldAttribute(int number) => Number = number;

    /// <summary>The stable field number as written in the declaration.</summary>
    public int Number { get; }
}
