using System.Globalization;
using System.Resources;
using Sferix.SPO.Abstractions.Providers;

namespace Sferix.SPO.Abstractions.Resources;

/// <summary>Reads Abstractions' user-visible messages from the embedded <c>AbstractionsMessages</c> resources (D-40, D-41).</summary>
/// <remarks>Messages never contain SQL text or values. An unsupported culture falls back to English.</remarks>
internal static class AbstractionsMessages
{
    private static readonly ResourceManager Resources =
        new("Sferix.SPO.Abstractions.Resources.AbstractionsMessages", typeof(AbstractionsMessages).Assembly);

    internal static string CapabilityMissing(string providerName, SpoCapability capability) =>
        string.Format(CultureInfo.CurrentUICulture, Get(nameof(CapabilityMissing)), providerName, capability);

    internal static string EntityIdEmpty => Get(nameof(EntityIdEmpty));

    private static string Get(string name) =>
        Resources.GetString(name, CultureInfo.CurrentUICulture)
        ?? throw new InvalidOperationException($"Missing Abstractions resource '{name}'.");
}
