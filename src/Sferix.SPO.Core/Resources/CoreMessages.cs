using System.Globalization;
using System.Resources;

namespace Sferix.SPO.Core.Resources;

/// <summary>Reads Core's user-visible messages from the embedded <c>CoreMessages</c> resources (D-40, D-41).</summary>
/// <remarks>Messages never contain SQL text or values. Lookups follow <see cref="CultureInfo.CurrentUICulture"/>; an unsupported culture falls back to English.</remarks>
internal static class CoreMessages
{
    private static readonly ResourceManager Resources =
        new("Sferix.SPO.Core.Resources.CoreMessages", typeof(CoreMessages).Assembly);

    internal static string SqlIdentifierInvalid => Get(nameof(SqlIdentifierInvalid));

    internal static string ModelDuplicateEntityId(Guid id) =>
        string.Format(CultureInfo.CurrentUICulture, Get(nameof(ModelDuplicateEntityId)), id);

    internal static string ModelInvalidName(string name) =>
        string.Format(CultureInfo.CurrentUICulture, Get(nameof(ModelInvalidName)), name);

    internal static string ModelDuplicateTableName(string name) =>
        string.Format(CultureInfo.CurrentUICulture, Get(nameof(ModelDuplicateTableName)), name);

    internal static string ModelUnsupportedStorageType(string name) =>
        string.Format(CultureInfo.CurrentUICulture, Get(nameof(ModelUnsupportedStorageType)), name);

    private static string Get(string name) =>
        Resources.GetString(name, CultureInfo.CurrentUICulture)
        ?? throw new InvalidOperationException($"Missing Core resource '{name}'.");
}
