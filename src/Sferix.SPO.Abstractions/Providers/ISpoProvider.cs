namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A storage provider as seen by Core.</summary>
/// <remarks>It declares what it can enforce; it does not open connections or render SQL here.</remarks>
public interface ISpoProvider
{
    /// <summary>A short provider name, used in messages.</summary>
    string Name { get; }

    /// <summary>The capabilities this provider enforces.</summary>
    IReadOnlySet<SpoCapability> Capabilities { get; }
}
