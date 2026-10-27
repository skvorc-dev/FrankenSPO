namespace Sferix.SPO.Core.Model;

/// <summary>Thrown at model activation when the metadata is invalid.</summary>
/// <remarks>Internal until an ObjectSpace surfaces activation; the message never contains SQL or values.</remarks>
internal sealed class SpoModelException(string code, string message) : Exception(message)
{
    public const string DuplicateEntityId = "SPO1002";
    public const string InvalidName = "SPO1003";

    /// <summary>The same code in every language.</summary>
    public string Code { get; } = code;
}
