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
    }

    public EntityMetadata Metadata { get; }
    public SchemaTable Table { get; }
    public int IdColumn { get; }
    public int CreatedColumn { get; }
    public int ModifiedColumn { get; }

    /// <summary>Whether the ObjectSpace can create, read and write this entity.</summary>
    public bool IsUsable =>
        IdColumn >= 0 && CreatedColumn >= 0 && ModifiedColumn >= 0 && Metadata.CreateInstance is not null
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

    public static bool SameValue(object? a, object? b) =>
        a is byte[] left && b is byte[] right ? left.AsSpan().SequenceEqual(right) : Equals(a, b);
}
