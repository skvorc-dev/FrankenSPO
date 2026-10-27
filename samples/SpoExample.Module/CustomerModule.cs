using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Modules;

namespace SpoExample.Module;

/// <summary>The module that contributes the sample's entities to a model.</summary>
public sealed class CustomerModule : ISpoModule
{
    public IEnumerable<EntityMetadata> Entities => [Customer.SpoMetadata];
}
