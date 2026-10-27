using System.Globalization;
using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Schema;

namespace Sferix.SPO.Provider.SQLite.Data;

/// <summary>Converts between CLR values and the stored SQLite formats of D-44.</summary>
/// <remarks>
/// Guid = 16-byte big-endian BLOB; decimal = invariant TEXT with scale kept; DateTime = UTC ticks (a non-UTC value fails);
/// DateTimeOffset = round-trip text; bool = 0/1. A value of the wrong CLR type throws <see cref="InvalidCastException"/>.
/// </remarks>
internal static class SqliteValueConverter
{
    public static object ToStored(SpoStorageType type, object? value, string table, string column)
    {
        if (value is null)
            return DBNull.Value;
        return type switch
        {
            SpoStorageType.Boolean => (bool)value ? 1L : 0L,
            SpoStorageType.Int32 => (long)(int)value,
            SpoStorageType.Int64 => (long)value,
            SpoStorageType.Decimal => ((decimal)value).ToString(CultureInfo.InvariantCulture),
            SpoStorageType.Double => (double)value,
            SpoStorageType.String => (string)value,
            SpoStorageType.Guid => KeyToStored((Guid)value),
            SpoStorageType.DateTime => ((DateTime)value) is { Kind: DateTimeKind.Utc } utc
                ? utc.Ticks
                : throw SpoValueException.DateTimeNotUtc(table, column),
            SpoStorageType.DateTimeOffset => ((DateTimeOffset)value).ToString("o", CultureInfo.InvariantCulture),
            SpoStorageType.Binary => (byte[])value,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }

    public static byte[] KeyToStored(Guid key) => key.ToByteArray(bigEndian: true);

    public static object? FromStored(SpoStorageType type, SqliteDataReader reader, int ordinal)
    {
        if (reader.IsDBNull(ordinal))
            return null;
        return type switch
        {
            SpoStorageType.Boolean => reader.GetInt64(ordinal) != 0,
            SpoStorageType.Int32 => checked((int)reader.GetInt64(ordinal)),
            SpoStorageType.Int64 => reader.GetInt64(ordinal),
            SpoStorageType.Decimal => decimal.Parse(reader.GetString(ordinal), NumberStyles.Number, CultureInfo.InvariantCulture),
            SpoStorageType.Double => reader.GetDouble(ordinal),
            SpoStorageType.String => reader.GetString(ordinal),
            SpoStorageType.Guid => new Guid((byte[])reader.GetValue(ordinal), bigEndian: true),
            SpoStorageType.DateTime => new DateTime(reader.GetInt64(ordinal), DateTimeKind.Utc),
            SpoStorageType.DateTimeOffset => DateTimeOffset.Parse(reader.GetString(ordinal), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
            SpoStorageType.Binary => (byte[])reader.GetValue(ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };
    }
}
