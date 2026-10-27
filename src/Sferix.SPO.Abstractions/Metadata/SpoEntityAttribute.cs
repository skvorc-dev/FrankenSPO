namespace Sferix.SPO.Abstractions.Metadata;

/// <summary>Marks a <c>partial</c> class as a persistent entity with a stable entity ID.</summary>
/// <remarks>
/// The ID is a GUID written once and never changed, so renaming the class does not change the entity's identity.
/// The source generator validates it; this attribute does not.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SpoEntityAttribute : Attribute
{
    /// <summary>Creates the attribute.</summary>
    /// <param name="id">The stable entity ID as a GUID string.</param>
    public SpoEntityAttribute(string id) => Id = id;

    /// <summary>The stable entity ID as written in the declaration.</summary>
    public string Id { get; }
}
