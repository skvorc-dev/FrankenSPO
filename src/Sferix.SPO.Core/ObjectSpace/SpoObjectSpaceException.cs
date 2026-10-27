namespace Sferix.SPO.Core.ObjectSpace;

/// <summary>Thrown when the ObjectSpace is used wrongly or a save finds no row to change.</summary>
/// <remarks>The message names the entity only, never SQL text, keys or values.</remarks>
public sealed class SpoObjectSpaceException : Exception
{
    /// <summary>The entity type is not part of the model (or cannot be created or keyed).</summary>
    public const string EntityNotInModel = "SPO1008";

    /// <summary>An update affected no row: the row was deleted or changed elsewhere.</summary>
    public const string NoRowAffected = "SPO1009";

    /// <summary>A call was made while another call on the same ObjectSpace was running.</summary>
    public const string ConcurrentUse = "SPO1010";

    /// <summary>The key of a tracked entity was changed after it was added or loaded.</summary>
    public const string KeyChanged = "SPO1011";

    /// <summary>The entity passed to <c>Delete</c> is not tracked by this ObjectSpace.</summary>
    public const string NotTracked = "SPO1012";

    internal SpoObjectSpaceException(string code, string message) : base(message) => Code = code;

    /// <summary>The stable error code, the same in every language.</summary>
    public string Code { get; }
}
