namespace Sferix.SPO.Core.Model;

/// <summary>Thrown at model activation when the metadata is invalid.</summary>
/// <remarks>
/// Codes: <see cref="DuplicateEntityId"/> (SPO1002) and <see cref="InvalidName"/> (SPO1003).
/// The message never contains SQL or values.
/// </remarks>
public sealed class SpoModelException : Exception
{
    /// <summary>Two entities share an entity ID. The same code in every language.</summary>
    public const string DuplicateEntityId = "SPO1002";

    /// <summary>An entity or property name is not a valid SQL identifier. The same code in every language.</summary>
    public const string InvalidName = "SPO1003";

    internal SpoModelException(string code, string message) : base(message) => Code = code;

    /// <summary>The stable error code, one of the constants on this type.</summary>
    public string Code { get; }
}
