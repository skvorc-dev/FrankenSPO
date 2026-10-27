namespace Sferix.SPO.Abstractions.Schema;

/// <summary>One column of a <see cref="SchemaTable"/>.</summary>
/// <param name="Name">The column name; Core has validated it as a SQL identifier.</param>
/// <param name="StorageType">The neutral storage type.</param>
/// <param name="IsNullable">Whether the column accepts null.</param>
public sealed record SchemaColumn(string Name, SpoStorageType StorageType, bool IsNullable);
