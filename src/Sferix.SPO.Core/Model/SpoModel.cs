using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.ObjectSpace;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Core.ObjectSpace;

namespace Sferix.SPO.Core.Model;

/// <summary>An activated model: the entities and schema that passed activation against a provider.</summary>
/// <remarks>Created only by <see cref="SpoModelBuilder.Build"/>. It opens ObjectSpaces; it exposes no query surface yet.</remarks>
public sealed class SpoModel
{
    private readonly ISpoProvider provider;
    private readonly SchemaModel schema;
    private readonly Dictionary<string, EntityInfo> byName;

    internal SpoModel(CompiledModel compiled, SchemaModel schema, ISpoProvider provider, TimeProvider timeProvider)
    {
        this.provider = provider;
        this.schema = schema;
        TimeProvider = timeProvider;
        Entities = compiled.Entities;
        // SchemaBuilder emits one table per entity, in entity order.
        byName = compiled.Entities
            .Select((entity, i) => new EntityInfo(entity, schema.Tables[i]))
            .ToDictionary(info => info.Metadata.ClrName, StringComparer.Ordinal);
    }

    /// <summary>The activated entities, in registration order.</summary>
    public IReadOnlyList<EntityMetadata> Entities { get; }

    internal TimeProvider TimeProvider { get; }

    internal EntityInfo? FindEntity(Type type) =>
        type.FullName is { } name && byName.TryGetValue(name.Replace('+', '.'), out var info) ? info : null;

    /// <summary>Opens a connection, makes sure the database has the model's schema, and returns a new ObjectSpace.</summary>
    /// <param name="connectionString">The provider's connection string.</param>
    /// <returns>An ObjectSpace that owns the connection; dispose it to close the connection.</returns>
    /// <remarks>An existing database that does not match the model is rejected and left unchanged.</remarks>
    public ISpoObjectSpace OpenObjectSpace(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var connection = provider.Open(connectionString);
        try
        {
            connection.EnsureSchema(schema);
            return new SpoObjectSpace(this, connection);
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }
}
