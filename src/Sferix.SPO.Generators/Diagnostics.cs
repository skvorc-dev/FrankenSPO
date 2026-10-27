using Microsoft.CodeAnalysis;
using Sferix.SPO.Generators.Resources;

namespace Sferix.SPO.Generators;

internal static class Diagnostics
{
    private const string Category = "Sferix.SPO";

    public static readonly DiagnosticDescriptor NotPartial = Error("SPO0001");

    public static readonly DiagnosticDescriptor InvalidEntityId = Error("SPO0002");

    public static readonly DiagnosticDescriptor DuplicateEntityId = Error("SPO0003");

    public static readonly DiagnosticDescriptor InvalidFieldNumber = Error("SPO0004");

    public static readonly DiagnosticDescriptor UnsupportedType = Error("SPO0005");

    public static readonly DiagnosticDescriptor MissingAccessor = Error("SPO0006");

    public static readonly DiagnosticDescriptor GenericEntity = Error("SPO0007");

    public static readonly DiagnosticDescriptor UnsupportedEntityKind = Error("SPO0008");

    public static readonly DiagnosticDescriptor UnsupportedEnumUnderlyingType = Error("SPO0009");

    private static DiagnosticDescriptor Error(string id) =>
        new(id, DiagnosticMessages.Localizable(id + "_Title"), DiagnosticMessages.Localizable(id + "_Message"),
            Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}
