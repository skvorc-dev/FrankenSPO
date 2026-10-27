using Sferix.SPO.Abstractions.Modules;
using Sferix.SPO.Abstractions.Providers;
using Sferix.SPO.Core.Schema;

namespace Sferix.SPO.Core.Model;

/// <summary>Registers modules and activates the model against a provider (D-36, D-43).</summary>
/// <remarks>
/// Activation runs <see cref="CompiledModel"/> checks and throws <see cref="SpoCapabilityMissingException"/>
/// (SPO1001) or <see cref="SpoModelException"/> (SPO1002, SPO1003, SPO1005, SPO1006: the schema is built here too). The builder is not thread-safe.
/// </remarks>
public sealed class SpoModelBuilder
{
    private static readonly IReadOnlySet<SpoCapability> RequiredCapabilities =
        new HashSet<SpoCapability> { SpoCapability.AtomicSaveChanges };

    private readonly List<ISpoModule> modules = [];
    private readonly HashSet<Type> registered = [];
    private TimeProvider timeProvider = TimeProvider.System;

    /// <summary>Registers a module. Registering the same module type again has no effect.</summary>
    /// <typeparam name="TModule">The module type; it must have a public parameterless constructor.</typeparam>
    /// <returns>This builder, for chaining.</returns>
    public SpoModelBuilder AddModule<TModule>() where TModule : ISpoModule, new()
    {
        if (registered.Add(typeof(TModule)))
            modules.Add(new TModule());
        return this;
    }

    /// <summary>Sets the clock used for new keys and the audit timestamps; the default is <see cref="TimeProvider.System"/>.</summary>
    /// <param name="timeProvider">The clock.</param>
    /// <returns>This builder, for chaining.</returns>
    public SpoModelBuilder UseTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        this.timeProvider = timeProvider;
        return this;
    }

    /// <summary>Activates the model against <paramref name="provider"/>.</summary>
    /// <param name="provider">The provider that must enforce the required capabilities.</param>
    /// <returns>The activated model.</returns>
    /// <exception cref="SpoCapabilityMissingException">The provider lacks a required capability (SPO1001).</exception>
    /// <exception cref="SpoModelException">Two entities share an ID (SPO1002) or a name is not a valid identifier (SPO1003).</exception>
    public SpoModel Build(ISpoProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var entities = modules.SelectMany(module => module.Entities);
        var compiled = CompiledModel.Build(entities, provider, RequiredCapabilities);
        return new SpoModel(compiled, SchemaBuilder.Build(compiled), provider, timeProvider);
    }
}
