namespace Sferix.SPO.Abstractions.Query;

/// <summary>Compares a column with a non-null value, with C# null semantics (see <see cref="SpoComparisonOperator"/>).</summary>
/// <param name="Column">The column name.</param>
/// <param name="Operator">The operator.</param>
/// <param name="Value">The value as a CLR value of the column's storage type; never null.</param>
public sealed record SpoComparisonPredicate(string Column, SpoComparisonOperator Operator, object Value) : SpoPredicate;
