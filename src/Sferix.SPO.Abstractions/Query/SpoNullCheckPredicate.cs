namespace Sferix.SPO.Abstractions.Query;

/// <summary>Tests whether a column is null.</summary>
/// <param name="Column">The column name.</param>
/// <param name="IsNull">True for "is null", false for "is not null".</param>
public sealed record SpoNullCheckPredicate(string Column, bool IsNull) : SpoPredicate;
