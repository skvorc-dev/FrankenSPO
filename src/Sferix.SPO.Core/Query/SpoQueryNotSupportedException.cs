using Sferix.SPO.Core.Resources;

namespace Sferix.SPO.Core.Query;

/// <summary>Thrown when a query uses something outside the supported subset, or a column that cannot be compared exactly.</summary>
/// <remarks>The message names the operator or member only, never SQL text or values (D-32, D-46).</remarks>
public sealed class SpoQueryNotSupportedException : Exception
{
    /// <summary>The same code in every language.</summary>
    public const string ErrorCode = "SPO1013";

    private SpoQueryNotSupportedException(string operatorName, string message) : base(message) => Operator = operatorName;

    /// <summary>The stable error code, <see cref="ErrorCode"/>.</summary>
    public string Code => ErrorCode;

    /// <summary>The operator, method or member that is not supported.</summary>
    public string Operator { get; }

    internal static SpoQueryNotSupportedException ForOperator(string operatorName) =>
        new(operatorName, CoreMessages.QueryNotSupported(operatorName));

    internal static SpoQueryNotSupportedException ForColumn(string column, string typeName) =>
        new($"{column} ({typeName})", CoreMessages.QueryColumnNotSupported(column, typeName));
}
