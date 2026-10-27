using System.Globalization;
using Sferix.SPO.Abstractions.Metadata;
using Sferix.SPO.Abstractions.Schema;

namespace Sferix.SPO.Core.ObjectSpace;

/// <summary>What the ObjectSpace needs to know about one entity: its table and the property behind each column.</summary>
/// <remarks>Built once at activation. Values move through the generated delegates; enums are converted to and from their integer.</remarks>
internal sealed class EntityInfo
{
    private readonly PropertyMetadata?[] properties;

    public EntityInfo(EntityMetadata metadata, SchemaTable table)
    {
        Metadata = metadata;
        Table = table;
        properties = table.Columns
            .Select(c => metadata.Properties.FirstOrDefault(p => p.ClrName == c.Name))
            .ToArray();
        IdColumn = table.PrimaryKey.Count == 1 && table.PrimaryKey[0] == "Id" ? table.Columns.ToList().FindIndex(c => c.Name == "Id") : -1;
        CreatedColumn = table.Columns.ToList().FindIndex(c => c.Name == "CreatedAt");
        ModifiedColumn = table.Columns.ToList().FindIndex(c => c.Name == "ModifiedAt");
        IsDeletedColumn = table.Columns.ToList().FindIndex(c => c.Name == "IsDeleted");
        // D-45: chosen once here, from the generated type; snapshots hold stored values, so an enum compares as its integer.
        comparers = properties.Select(p => p is null ? Equals : ComparerFor(p.EnumUnderlyingType ?? p.ClrType)).ToArray();
    }

    private readonly Func<object?, object?, bool>[] comparers;

    public EntityMetadata Metadata { get; }
    public SchemaTable Table { get; }
    public int IdColumn { get; }
    public int CreatedColumn { get; }
    public int ModifiedColumn { get; }
    public int IsDeletedColumn { get; }

    /// <summary>Whether the ObjectSpace can create, read and write this entity.</summary>
    public bool IsUsable =>
        IdColumn >= 0 && CreatedColumn >= 0 && ModifiedColumn >= 0 && IsDeletedColumn >= 0 && Metadata.CreateInstance is not null
        && properties.All(p => p is { GetValue: not null, SetValue: not null });

    public object?[] ReadValues(object entity)
    {
        var values = new object?[properties.Length];
        for (var i = 0; i < values.Length; i++)
        {
            var property = properties[i]!;
            var value = property.GetValue!(entity);
            values[i] = value is null || property.EnumUnderlyingType is null
                ? value
                : Table.Columns[i].StorageType == SpoStorageType.Int64
                    ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                    : (object)Convert.ToInt32(value, CultureInfo.InvariantCulture); // cast: a bare ternary would widen to long
        }
        return values;
    }

    public void WriteValues(object entity, IReadOnlyList<object?> stored)
    {
        for (var i = 0; i < properties.Length; i++)
        {
            var property = properties[i]!;
            var value = stored[i];
            property.SetValue!(entity, value is null || property.EnumUnderlyingType is null
                ? value
                : Convert.ChangeType(value, property.EnumUnderlyingType, CultureInfo.InvariantCulture));
        }
    }

    /// <summary>The column index of the persistent property named <paramref name="propertyName"/>, or -1.</summary>
    public int FindColumn(string propertyName) =>
        Array.FindIndex(properties, p => p is not null && p.ClrName == propertyName);

    public PropertyMetadata PropertyAt(int column) => properties[column]!;

    public bool SameValue(int column, object? a, object? b) => comparers[column](a, b);

    /// <summary>The exact comparer for values of <paramref name="type"/> (D-30, D-45); null equals only null.</summary>
    internal static Func<object?, object?, bool> ComparerFor(Type type)
    {
        Func<object, object, bool> exact =
            type == typeof(decimal) ? (a, b) => (decimal)a == (decimal)b && Scale((decimal)a) == Scale((decimal)b)
            : type == typeof(DateTime) ? (a, b) => ((DateTime)a).Ticks == ((DateTime)b).Ticks && ((DateTime)a).Kind == ((DateTime)b).Kind
            : type == typeof(DateTimeOffset) ? (a, b) => ((DateTimeOffset)a).Ticks == ((DateTimeOffset)b).Ticks && ((DateTimeOffset)a).Offset == ((DateTimeOffset)b).Offset
            : type == typeof(byte[]) ? (a, b) => ((byte[])a).AsSpan().SequenceEqual((byte[])b)
            : type == typeof(string) ? (a, b) => string.Equals((string)a, (string)b, StringComparison.Ordinal)
            : (a, b) => a.Equals(b);
        return (a, b) => a is null || b is null ? a is null && b is null : exact(a, b);
    }

    private static int Scale(decimal value) => (decimal.GetBits(value)[3] >> 16) & 0xFF;
}
