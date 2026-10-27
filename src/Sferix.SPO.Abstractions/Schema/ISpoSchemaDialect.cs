namespace Sferix.SPO.Abstractions.Schema;

/// <summary>Renders DDL for one provider.</summary>
/// <remarks>It renders text only and never executes it. Identifiers come from validated metadata.</remarks>
public interface ISpoSchemaDialect
{
    /// <summary>Renders the statement that creates <paramref name="table"/>.</summary>
    /// <param name="table">The table to create.</param>
    string RenderCreateTable(SchemaTable table);
}
