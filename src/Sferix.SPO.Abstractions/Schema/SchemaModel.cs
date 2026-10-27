using System.Collections.ObjectModel;

namespace Sferix.SPO.Abstractions.Schema;

/// <summary>The provider-neutral description of the tables a model needs.</summary>
/// <remarks>Immutable. Core builds it from the compiled model; a provider renders and checks it.</remarks>
public sealed class SchemaModel
{
    /// <summary>Creates the model; the table list is copied.</summary>
    /// <param name="tables">The tables.</param>
    public SchemaModel(IEnumerable<SchemaTable> tables)
    {
        ArgumentNullException.ThrowIfNull(tables);
        Tables = new ReadOnlyCollection<SchemaTable>(tables.ToArray());
    }

    /// <summary>The tables in a stable order.</summary>
    public IReadOnlyList<SchemaTable> Tables { get; }
}
