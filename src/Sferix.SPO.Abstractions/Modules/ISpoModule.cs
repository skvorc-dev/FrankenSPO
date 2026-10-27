using Sferix.SPO.Abstractions.Metadata;

namespace Sferix.SPO.Abstractions.Modules;

/// <summary>An application module that contributes entities to a model (D-36).</summary>
/// <remarks>Modules are registered explicitly with <c>SpoModelBuilder.AddModule</c>; there is no assembly scanning.</remarks>
public interface ISpoModule
{
    /// <summary>The entities this module contributes.</summary>
    IEnumerable<EntityMetadata> Entities { get; }
}
