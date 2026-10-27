namespace Sferix.SPO.Abstractions.Query;

/// <summary>One ordering key.</summary>
/// <param name="Column">The column name.</param>
/// <param name="Descending">True for descending order. Null values sort first when ascending, as in C#.</param>
public sealed record SpoOrdering(string Column, bool Descending);
