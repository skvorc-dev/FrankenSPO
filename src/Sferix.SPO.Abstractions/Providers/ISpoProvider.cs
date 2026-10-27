namespace Sferix.SPO.Abstractions.Providers;

/// <summary>A storage provider as seen by Core.</summary>
/// <remarks>It declares what it can enforce and opens connections. Sync and async members do the same work; the async ones never block on async code.</remarks>
public interface ISpoProvider
{
    /// <summary>A short provider name, used in messages.</summary>
    string Name { get; }

    /// <summary>The capabilities this provider enforces.</summary>
    IReadOnlySet<SpoCapability> Capabilities { get; }

    /// <summary>Opens a connection with the provider's settings applied.</summary>
    /// <param name="connectionString">The provider's connection string.</param>
    /// <remarks>Raw SQL is not part of this contract; the connection offers row access only.</remarks>
    ISpoConnection Open(string connectionString);
}
