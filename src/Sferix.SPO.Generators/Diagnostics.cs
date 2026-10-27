using Microsoft.CodeAnalysis;

namespace Sferix.SPO.Generators;

internal static class Diagnostics
{
    private const string Category = "Sferix.SPO";

    public static readonly DiagnosticDescriptor NotPartial = Error(
        "SPO0001", "Entity class must be partial",
        "Entity '{0}' and every class containing it must be declared 'partial'");

    public static readonly DiagnosticDescriptor InvalidEntityId = Error(
        "SPO0002", "Entity ID missing or not a GUID",
        "Entity '{0}' needs [SpoEntity] with a non-empty GUID string, for example [SpoEntity(\"{1}\")]");

    public static readonly DiagnosticDescriptor DuplicateEntityId = Error(
        "SPO0003", "Entity ID used twice",
        "Entity ID {1} is used by more than one entity, including '{0}'");

    public static readonly DiagnosticDescriptor InvalidFieldNumber = Error(
        "SPO0004", "Invalid [SpoField] number",
        "Property '{0}' has [SpoField({1})]: {2}");

    public static readonly DiagnosticDescriptor UnsupportedType = Error(
        "SPO0005", "Unsupported property type",
        "Property '{0}' has type '{1}', which is not supported for persistence");

    public static readonly DiagnosticDescriptor MissingAccessor = Error(
        "SPO0006", "Persistent property needs getter and setter",
        "Property '{0}' has [SpoField] but lacks a getter or a setter");

    private static DiagnosticDescriptor Error(string id, string title, string message) =>
        new(id, title, message, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);
}
