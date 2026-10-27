namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A runtime capability a provider can declare and a model can require (D-39).</summary>
/// <remarks>A name is added only together with the code that first checks it.</remarks>
public enum SpoCapability
{
    /// <summary>Saving a set of changes either applies all of them or none.</summary>
    AtomicSaveChanges,
}
