using Sferix.SPO.Abstractions.Schema;

namespace Sferix.SPO.Abstractions.Query;

/// <summary>A provider-neutral query over one table: filter, order and page.</summary>
/// <remarks>It selects whole rows in schema column order. The provider never receives expression trees or SQL text from callers.</remarks>
/// <param name="Table">The table to read.</param>
/// <param name="Where">The filter, or null for all rows.</param>
/// <param name="OrderBy">The ordering keys, in order.</param>
/// <param name="Skip">Rows to skip, or null.</param>
/// <param name="Take">Maximum rows to return, or null; zero returns no rows.</param>
public sealed record SpoQuery(SchemaTable Table, SpoPredicate? Where, IReadOnlyList<SpoOrdering> OrderBy, int? Skip, int? Take);
