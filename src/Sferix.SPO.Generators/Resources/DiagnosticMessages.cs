using System.Resources;
using Microsoft.CodeAnalysis;

namespace Sferix.SPO.Generators.Resources;

/// <summary>Reads generator diagnostic texts from the embedded <c>DiagnosticMessages</c> resources (D-40, D-41).</summary>
/// <remarks>Lookups follow the UI culture of the compiler; an unsupported culture falls back to English.</remarks>
internal static class DiagnosticMessages
{
    private static readonly ResourceManager Manager =
        new("Sferix.SPO.Generators.Resources.DiagnosticMessages", typeof(DiagnosticMessages).Assembly);

    public static LocalizableString Localizable(string name) =>
        new LocalizableResourceString(name, Manager, typeof(DiagnosticMessages));

    /// <summary>Text for message arguments; resolved by Roslyn, which owns the culture lookup.</summary>
    public static string Get(string name) => Localizable(name).ToString();
}
