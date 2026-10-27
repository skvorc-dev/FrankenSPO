using Sferix.SPO.Abstractions.Resources;

namespace Sferix.SPO.Abstractions.Providers;

/// <summary>Thrown at model activation when a provider lacks a capability the model requires.</summary>
/// <remarks>The message names the provider and the capability only; it never contains SQL or values.</remarks>
public sealed class SpoCapabilityMissingException : Exception
{
    /// <summary>The same code in every language.</summary>
    public const string ErrorCode = "SPO1001";

    /// <summary>Creates the exception.</summary>
    /// <param name="capability">The missing capability.</param>
    /// <param name="providerName">The provider's name.</param>
    public SpoCapabilityMissingException(SpoCapability capability, string providerName)
        : base(AbstractionsMessages.CapabilityMissing(providerName, capability))
    {
        Capability = capability;
        ProviderName = providerName;
    }

    /// <summary>The stable error code, <see cref="ErrorCode"/>.</summary>
    public string Code => ErrorCode;

    /// <summary>The capability the provider lacks.</summary>
    public SpoCapability Capability { get; }

    /// <summary>The provider's name.</summary>
    public string ProviderName { get; }
}
