using Sferix.SPO.Abstractions.Metadata;

namespace Sferix.SPO.Core.Model;

/// <summary>An activated model: the entities that passed activation against a provider.</summary>
/// <remarks>Created only by <see cref="SpoModelBuilder.Build"/>. It exposes no schema and no query surface yet.</remarks>
public sealed class SpoModel
{
    internal SpoModel(CompiledModel compiled) => Entities = compiled.Entities;

    /// <summary>The activated entities, in registration order.</summary>
    public IReadOnlyList<EntityMetadata> Entities { get; }
}
