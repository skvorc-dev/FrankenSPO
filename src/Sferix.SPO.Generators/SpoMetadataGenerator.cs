using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Sferix.SPO.Generators;

/// <summary>Emits a static <c>SpoMetadata</c> property into every <c>[SpoEntity]</c> partial class.</summary>
/// <remarks>
/// Reads only <c>[SpoEntity]</c> and <c>[SpoField]</c>. Reports invalid declarations as errors and emits nothing for
/// an entity that has one. The generated code uses no reflection.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class SpoMetadataGenerator : IIncrementalGenerator
{
    private const string EntityAttribute = "Sferix.SPO.Abstractions.Metadata.SpoEntityAttribute";
    private const string FieldAttribute = "Sferix.SPO.Abstractions.Metadata.SpoFieldAttribute";
    internal const string TrackingName = "SpoEntities";
    private const string MetadataNamespace = "global::Sferix.SPO.Abstractions.Metadata";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entities = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                EntityAttribute,
                static (node, _) => node is TypeDeclarationSyntax,
                static (ctx, ct) => Analyze(ctx, ct))
            .WithTrackingName(TrackingName)
            .Collect();

        context.RegisterSourceOutput(entities, static (spc, models) => Emit(spc, models));
    }

    private static EntityModel Analyze(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        var symbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var declaration = (TypeDeclarationSyntax)ctx.TargetNode;
        var displayName = symbol.ToDisplayString();
        var classLocation = LocationInfo.From(declaration.Identifier.GetLocation());
        var diagnostics = new List<DiagnosticInfo>();

        if (symbol.TypeKind != TypeKind.Class || symbol.IsRecord)
        {
            // Only plain classes are supported; stop here so nothing is emitted for other shapes.
            diagnostics.Add(new(Diagnostics.UnsupportedEntityKind, classLocation, Args(displayName)));
            return Rejected(symbol, displayName, classLocation, diagnostics);
        }

        if (IsGenericChain(symbol))
        {
            diagnostics.Add(new(Diagnostics.GenericEntity, classLocation, Args(displayName)));
            return Rejected(symbol, displayName, classLocation, diagnostics);
        }

        if (!IsPartialChain(symbol, declaration))
            diagnostics.Add(new(Diagnostics.NotPartial, classLocation, Args(displayName)));

        var attribute = ctx.Attributes[0];
        Guid? id = null;
        var raw = attribute.ConstructorArguments.Length > 0 ? attribute.ConstructorArguments[0].Value as string : null;
        if (raw is not null && Guid.TryParse(raw, out var parsed) && parsed != Guid.Empty)
            id = parsed;
        else
        {
            var location = attribute.ApplicationSyntaxReference?.GetSyntax(ct).GetLocation();
            diagnostics.Add(new(
                Diagnostics.InvalidEntityId,
                location is null ? classLocation : LocationInfo.From(location),
                Args(displayName)));
        }

        var properties = new List<PropertyModel>();
        var usedNumbers = new HashSet<int>();
        foreach (var member in symbol.GetMembers())
        {
            ct.ThrowIfCancellationRequested();
            if (member is not IPropertySymbol { IsStatic: false, IsIndexer: false } property)
                continue;
            var field = property.GetAttributes().FirstOrDefault(a => a.AttributeClass?.ToDisplayString() == FieldAttribute);
            if (field is null)
                continue;

            var location = property.Locations.Length > 0 ? LocationInfo.From(property.Locations[0]) : classLocation;
            var number = field.ConstructorArguments.Length > 0 && field.ConstructorArguments[0].Value is int n ? n : 0;
            var valid = true;

            if (number <= 0)
            {
                diagnostics.Add(new(Diagnostics.InvalidFieldNumber, location, Args(property.Name, number.ToString(), "the number must be positive")));
                valid = false;
            }
            else if (!usedNumbers.Add(number))
            {
                diagnostics.Add(new(Diagnostics.InvalidFieldNumber, location, Args(property.Name, number.ToString(), "the number is already used in this entity")));
                valid = false;
            }

            if (property.GetMethod is null || property.SetMethod is null)
            {
                diagnostics.Add(new(Diagnostics.MissingAccessor, location, Args(property.Name)));
                valid = false;
            }

            if (!TryMapType(property.Type, out var typeExpression, out var nullable))
            {
                diagnostics.Add(new(Diagnostics.UnsupportedType, location, Args(property.Name, property.Type.ToDisplayString())));
                valid = false;
            }

            if (valid)
                properties.Add(new(number, property.Name, typeExpression, nullable));
        }

        var containers = new List<ContainerInfo>();
        for (var container = symbol.ContainingType; container is not null; container = container.ContainingType)
            containers.Insert(0, new(Keyword(container), container.Name));

        return new EntityModel(
            symbol.ContainingNamespace.IsGlobalNamespace ? null : symbol.ContainingNamespace.ToDisplayString(),
            new(containers.ToImmutableArray()),
            symbol.Name,
            displayName,
            id,
            classLocation,
            new(properties.OrderBy(p => p.Number).ToImmutableArray()),
            new(diagnostics.ToImmutableArray()));
    }

    private static EntityModel Rejected(
        INamedTypeSymbol symbol, string displayName, LocationInfo location, List<DiagnosticInfo> diagnostics) =>
        new(null, default, symbol.Name, displayName, null, location, default, new(diagnostics.ToImmutableArray()));

    private static bool IsGenericChain(INamedTypeSymbol symbol)
    {
        for (var type = symbol; type is not null; type = type.ContainingType)
            if (type.IsGenericType)
                return true;
        return false;
    }

    private static EquatableArray<string> Args(params string[] args) => new(args.ToImmutableArray());

    private static bool IsPartialChain(INamedTypeSymbol symbol, TypeDeclarationSyntax declaration)
    {
        if (!declaration.Modifiers.Any(SyntaxKind.PartialKeyword))
            return false;
        for (var container = symbol.ContainingType; container is not null; container = container.ContainingType)
        {
            var partial = container.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<TypeDeclarationSyntax>()
                .Any(d => d.Modifiers.Any(SyntaxKind.PartialKeyword));
            if (!partial)
                return false;
        }
        return true;
    }

    private static string Keyword(INamedTypeSymbol type) => type switch
    {
        { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
        { IsRecord: true } => "record",
        { TypeKind: TypeKind.Struct } => "struct",
        { TypeKind: TypeKind.Interface } => "interface",
        _ => "class",
    };

    private static bool TryMapType(ITypeSymbol type, out string typeExpression, out bool nullable)
    {
        var inner = type;
        nullable = false;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } named)
        {
            inner = named.TypeArguments[0];
            nullable = true;
        }
        else if (type.IsReferenceType)
            nullable = type.NullableAnnotation == NullableAnnotation.Annotated;

        inner = inner.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
        typeExpression = inner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        return inner.TypeKind == TypeKind.Enum
            || inner.SpecialType is SpecialType.System_Boolean or SpecialType.System_Int32 or SpecialType.System_Int64
                or SpecialType.System_Decimal or SpecialType.System_Double or SpecialType.System_String
                or SpecialType.System_DateTime
            || inner.ToDisplayString() is "System.Guid" or "System.DateTimeOffset"
            || inner is IArrayTypeSymbol { Rank: 1, ElementType.SpecialType: SpecialType.System_Byte };
    }

    private static void Emit(SourceProductionContext spc, ImmutableArray<EntityModel> models)
    {
        var duplicates = models
            .Where(m => m.Id is not null)
            .GroupBy(m => m.Id!.Value)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.Select(m => (Model: m, Id: g.Key)))
            .ToDictionary(x => x.Model.DisplayName, x => x.Id);

        foreach (var model in models)
        {
            foreach (var diagnostic in model.Diagnostics.Items)
            {
                // The suggested GUID is created here, not in the model, so the model stays equal between runs.
                spc.ReportDiagnostic(ReferenceEquals(diagnostic.Descriptor, Diagnostics.InvalidEntityId)
                    ? Diagnostic.Create(diagnostic.Descriptor, diagnostic.Location.ToLocation(), model.DisplayName, Guid.NewGuid().ToString("D"))
                    : diagnostic.ToDiagnostic());
            }

            if (duplicates.TryGetValue(model.DisplayName, out var duplicateId))
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    Diagnostics.DuplicateEntityId, model.ClassLocation.ToLocation(), model.DisplayName, duplicateId.ToString("D")));
                continue;
            }

            if (model.Diagnostics.Items.Length == 0 && model.Id is { } id)
                spc.AddSource(HintName(model), Render(model, id));
        }
    }

    private static string HintName(EntityModel model)
    {
        var name = new StringBuilder();
        foreach (var ch in model.DisplayName)
            name.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        // Sanitizing maps different names to the same text (A.B_C, A_B.C); the hash of the real name keeps them apart.
        return name + "_" + StableHash(model.DisplayName) + ".SpoMetadata.g.cs";
    }

    private static string StableHash(string text)
    {
        var hash = 2166136261u;
        foreach (var ch in text)
            hash = unchecked((hash ^ ch) * 16777619u);
        return hash.ToString("x8");
    }

    private static string Render(EntityModel model, Guid id)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        if (model.Namespace is not null)
            sb.Append("namespace ").Append(model.Namespace).AppendLine(" {");
        foreach (var container in model.Containers.Items)
            sb.Append("partial ").Append(container.Keyword).Append(' ').Append(container.Name).AppendLine(" {");

        sb.Append("partial class ").Append(model.Name).AppendLine(" {");
        sb.Append("    /// <summary>Generated metadata of this entity.</summary>").AppendLine();
        sb.Append("    public static ").Append(MetadataNamespace).Append(".EntityMetadata SpoMetadata { get; } = new ")
          .Append(MetadataNamespace).AppendLine(".EntityMetadata(");
        sb.Append("        new ").Append(MetadataNamespace).Append(".EntityId(new global::System.Guid(\"").Append(id.ToString("D")).AppendLine("\")),");
        sb.Append("        ").Append(SymbolDisplay.FormatLiteral(model.DisplayName, quote: true)).AppendLine(",");
        sb.Append("        new ").Append(MetadataNamespace).AppendLine(".PropertyMetadata[]");
        sb.AppendLine("        {");
        foreach (var p in model.Properties.Items)
        {
            sb.Append("            new ").Append(MetadataNamespace).Append(".PropertyMetadata(")
              .Append(p.Number).Append(", ").Append(SymbolDisplay.FormatLiteral(p.Name, quote: true))
              .Append(", typeof(").Append(p.ClrTypeExpression).Append("), ").Append(p.IsNullable ? "true" : "false").AppendLine("),");
        }
        sb.AppendLine("        });");
        sb.AppendLine("}");

        foreach (var _ in model.Containers.Items)
            sb.AppendLine("}");
        if (model.Namespace is not null)
            sb.AppendLine("}");
        return sb.ToString();
    }
}
