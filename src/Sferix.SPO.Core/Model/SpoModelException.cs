namespace Sferix.SPO.Core.Model;

/// <summary>Thrown at model activation when the metadata is invalid.</summary>
/// <remarks>
/// Codes: <see cref="DuplicateEntityId"/> (SPO1002), <see cref="InvalidName"/> (SPO1003),
/// <see cref="DuplicateTableName"/> (SPO1005) and <see cref="UnsupportedStorageType"/> (SPO1006).
/// The message never contains SQL or values.
/// </remarks>
public sealed class SpoModelException : Exception
{
    /// <summary>Two entities share an entity ID. The same code in every language.</summary>
    public const string DuplicateEntityId = "SPO1002";

    /// <summary>An entity or property name is not a valid SQL identifier. The same code in every language.</summary>
    public const string InvalidName = "SPO1003";

    /// <summary>Two entities map to the same table name. The same code in every language.</summary>
    public const string DuplicateTableName = "SPO1005";

    /// <summary>A property has no storage type in the schema. The same code in every language.</summary>
    public const string UnsupportedStorageType = "SPO1006";

    internal SpoModelException(string code, string message) : base(message) => Code = code;

    /// <summary>The stable error code, one of the constants on this type.</summary>
    public string Code { get; }
}
