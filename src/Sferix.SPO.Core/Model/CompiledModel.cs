using System.Collections.ObjectModel;
using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Core.Resources;
using Sferix.SPO.Core.Sql;

namespace Sferix.SPO.Core.Model;

/// <summary>The validated model: generated metadata that passed activation against a provider.</summary>
/// <remarks>Building it is activation (D-33): every check runs here, once, and fails closed. It does not build a schema.</remarks>
internal sealed class CompiledModel
{
    private CompiledModel(IReadOnlyList<EntityMetadata> entities) => Entities = entities;

    public IReadOnlyList<EntityMetadata> Entities { get; }

    public static CompiledModel Build(
        IEnumerable<EntityMetadata> entities, ISpoProvider provider, IReadOnlySet<SpoCapability> required)
    {
        ArgumentNullException.ThrowIfNull(entities);
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(required);

        foreach (var capability in required.Order())
            if (!provider.Capabilities.Contains(capability))
                throw new SpoCapabilityMissingException(capability, provider.Name);

        var list = entities.ToArray();
        var ids = new HashSet<EntityId>();
        foreach (var entity in list)
        {
            if (!ids.Add(entity.Id))
                throw new SpoModelException(
                    SpoModelException.DuplicateEntityId, CoreMessages.ModelDuplicateEntityId(entity.Id.Value));

            RequireIdentifier(entity.ClrName[(entity.ClrName.LastIndexOf('.') + 1)..]);
            foreach (var property in entity.Properties)
                RequireIdentifier(property.ClrName);
        }

        return new CompiledModel(new ReadOnlyCollection<EntityMetadata>(list));
    }

    private static void RequireIdentifier(string name)
    {
        try
        {
            _ = new SqlIdentifier(name);
        }
        catch (ArgumentException)
        {
            throw new SpoModelException(SpoModelException.InvalidName, CoreMessages.ModelInvalidName(name));
        }
    }
}
