using System.Collections.ObjectModel;

namespace Sferix.SPO.Abstractions.Schema;

/// <summary>One table of a <see cref="SchemaModel"/>.</summary>
/// <remarks>Immutable; the lists are copied. It describes no indexes or relations.</remarks>
public sealed class SchemaTable
{
    /// <summary>Creates the table description.</summary>
    /// <param name="name">The table name.</param>
    /// <param name="columns">The columns in order.</param>
    /// <param name="primaryKey">The names of the primary-key columns; empty when there is none yet.</param>
    public SchemaTable(string name, IEnumerable<SchemaColumn> columns, IEnumerable<string>? primaryKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(columns);
        Name = name;
        Columns = new ReadOnlyCollection<SchemaColumn>(columns.ToArray());
        PrimaryKey = new ReadOnlyCollection<string>(primaryKey?.ToArray() ?? []);
    }

    /// <summary>The table name.</summary>
    public string Name { get; }

    /// <summary>The columns in order.</summary>
    public IReadOnlyList<SchemaColumn> Columns { get; }

    /// <summary>The primary-key column names; empty when there is none.</summary>
    public IReadOnlyList<string> PrimaryKey { get; }
}
