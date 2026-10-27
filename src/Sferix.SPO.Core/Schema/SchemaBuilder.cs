using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Core.Model;
using Sferix.SPO.Core.Resources;
using Sferix.SPO.Core.Sql;

namespace Sferix.SPO.Core.Schema;

/// <summary>Builds the provider-neutral schema from a compiled model.</summary>
/// <remarks>Table = entity class name, column = property name. Output order is the model order, then field number. Nothing is written.</remarks>
internal static class SchemaBuilder
{
    public const string DuplicateTableName = "SPO1005";
    public const string UnsupportedStorageType = "SPO1006";

    public static SchemaModel Build(CompiledModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var tables = new List<SchemaTable>();
        foreach (var entity in model.Entities)
        {
            var name = new SqlIdentifier(entity.ClrName[(entity.ClrName.LastIndexOf('.') + 1)..]).Value;
            if (!names.Add(name))
                throw new SpoModelException(DuplicateTableName, CoreMessages.ModelDuplicateTableName(name));
            var columns = entity.Properties
                .OrderBy(p => p.Number)
                .Select(p => new SchemaColumn(new SqlIdentifier(p.ClrName).Value, StorageTypeOf(p), p.IsNullable));
            tables.Add(new SchemaTable(name, columns));
        }
        return new SchemaModel(tables);
    }

    private static SpoStorageType StorageTypeOf(PropertyMetadata property)
    {
        var type = property.EnumUnderlyingType ?? property.ClrType;
        if (property.EnumUnderlyingType is not null)
            return type == typeof(uint) || type == typeof(long) ? SpoStorageType.Int64
                : IsSmallInteger(type) ? SpoStorageType.Int32 : Unsupported(property);
        if (type == typeof(bool)) return SpoStorageType.Boolean;
        if (type == typeof(int)) return SpoStorageType.Int32;
        if (type == typeof(long)) return SpoStorageType.Int64;
        if (type == typeof(decimal)) return SpoStorageType.Decimal;
        if (type == typeof(double)) return SpoStorageType.Double;
        if (type == typeof(string)) return SpoStorageType.String;
        if (type == typeof(Guid)) return SpoStorageType.Guid;
        if (type == typeof(DateTime)) return SpoStorageType.DateTime;
        if (type == typeof(DateTimeOffset)) return SpoStorageType.DateTimeOffset;
        if (type == typeof(byte[])) return SpoStorageType.Binary;
        return Unsupported(property);
    }

    private static bool IsSmallInteger(Type type) =>
        type == typeof(byte) || type == typeof(sbyte) || type == typeof(short) || type == typeof(ushort) || type == typeof(int);

    private static SpoStorageType Unsupported(PropertyMetadata property) =>
        throw new SpoModelException(UnsupportedStorageType, CoreMessages.ModelUnsupportedStorageType(property.ClrName));
}
