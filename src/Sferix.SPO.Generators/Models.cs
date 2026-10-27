using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Sferix.SPO.Generators;

internal readonly record struct LocationInfo(string FilePath, TextSpan Span, LinePositionSpan LineSpan)
{
    public static LocationInfo From(Location location) =>
        new(location.SourceTree?.FilePath ?? string.Empty, location.SourceSpan, location.GetLineSpan().Span);

    public Location ToLocation() => Location.Create(FilePath, Span, LineSpan);
}

internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, LocationInfo Location, EquatableArray<string> Args)
{
    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor, Location.ToLocation(), Args.Items.ToArray<object?>());
}

internal sealed record ContainerInfo(string Keyword, string Name);

internal sealed record PropertyModel(int Number, string Name, string ClrTypeExpression, bool IsNullable, string? EnumUnderlyingTypeExpression);

internal sealed record EntityModel(
    string? Namespace,
    EquatableArray<ContainerInfo> Containers,
    string Name,
    string DisplayName,
    Guid? Id,
    LocationInfo ClassLocation,
    EquatableArray<PropertyModel> Properties,
    EquatableArray<DiagnosticInfo> Diagnostics);

/// <summary>An array with value equality, so incremental pipeline steps can be cached.</summary>
internal readonly struct EquatableArray<T>(ImmutableArray<T> items) : IEquatable<EquatableArray<T>>
{
    public ImmutableArray<T> Items { get; } = items.IsDefault ? ImmutableArray<T>.Empty : items;

    public bool Equals(EquatableArray<T> other) =>
        Items.Length == other.Items.Length && Items.SequenceEqual(other.Items);

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        var hash = Items.Length;
        foreach (var item in Items)
            hash = unchecked(hash * 31 + (item?.GetHashCode() ?? 0));
        return hash;
    }
}
