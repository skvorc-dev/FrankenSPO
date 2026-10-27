namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A new value for one column of a row.</summary>
/// <param name="Column">The column name as in the <see cref="Schema.SchemaTable"/>.</param>
/// <param name="Value">The value as a CLR value of the column's storage type, or null.</param>
public readonly record struct SpoColumnValue(string Column, object? Value);
