namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A runtime capability a provider can declare and a model can require (D-39).</summary>
/// <remarks>A name is added only together with the code that first checks it.</remarks>
public enum SpoCapability
{
    /// <summary>Saving a set of changes either applies all of them or none.</summary>
    AtomicSaveChanges,

    /// <summary>The provider can enforce a tenant restriction in every affected query.</summary>
    /// <remarks>No P1 provider declares it; a model that requires it fails at activation (D-33, rule 10).</remarks>
    TenantPolicy,

    /// <summary>The provider can enforce a row restriction in every affected query.</summary>
    /// <remarks>No P1 provider declares it; a model that requires it fails at activation (D-33, rule 10).</remarks>
    RowPolicy,

    /// <summary>The provider can enforce a field restriction in every affected query.</summary>
    /// <remarks>No P1 provider declares it; a model that requires it fails at activation (D-33, rule 10).</remarks>
    FieldPolicy,
}
