using System.Text;
using Microsoft.Data.Sqlite;
using Sferix.SPO.Abstractions.Query;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Provider.SQLite.Schema;

namespace Sferix.SPO.Provider.SQLite.Data;

/// <summary>Renders a neutral query as one parameterized SELECT.</summary>
/// <remarks>
/// Identifiers come from the query's table and are validated and quoted; every value is a parameter. Every predicate renders two-valued
/// (D-46): equality uses <c>IS</c>, ordering comparisons on nullable columns are wrapped in <c>COALESCE</c>. String matching uses
/// <c>substr</c>/<c>instr</c> with a parameter and never LIKE, so it is ordinal, case-sensitive and wildcard-free (D-45).
/// </remarks>
internal sealed class SqliteQueryRenderer(SqliteCommand command, SchemaTable table)
{
    private int parameters;

    public static void Render(SqliteCommand command, SpoQuery query)
    {
        var renderer = new SqliteQueryRenderer(command, query.Table);
        var sql = new StringBuilder("SELECT ")
            .Append(string.Join(", ", query.Table.Columns.Select(c => SqliteSchemaDialect.Quote(c.Name))))
            .Append(" FROM ").Append(SqliteSchemaDialect.Quote(query.Table.Name));
        if (query.Where is not null)
            sql.Append(" WHERE ").Append(renderer.Predicate(query.Where));
        if (query.OrderBy.Count > 0)
            sql.Append(" ORDER BY ").Append(string.Join(", ", query.OrderBy.Select(o =>
                SqliteSchemaDialect.Quote(renderer.Column(o.Column).Name) + (o.Descending ? " DESC" : " ASC"))));
        if (query.Take is not null || query.Skip is not null)
        {
            // OFFSET needs a LIMIT in SQLite; -1 means no limit.
            sql.Append(" LIMIT ").Append(renderer.Parameter(query.Take ?? -1));
            if (query.Skip is { } skip)
                sql.Append(" OFFSET ").Append(renderer.Parameter(skip));
        }
        command.CommandText = sql.ToString();
    }

    private string Predicate(SpoPredicate predicate)
    {
        switch (predicate)
        {
            case SpoConstantPredicate c:
                return c.Value ? "1" : "0";
            case SpoAndPredicate a:
                return $"({Predicate(a.Left)} AND {Predicate(a.Right)})";
            case SpoOrPredicate o:
                return $"({Predicate(o.Left)} OR {Predicate(o.Right)})";
            case SpoNotPredicate n:
                return $"(NOT {Predicate(n.Operand)})";
            case SpoNullCheckPredicate n:
                return $"({Quote(n.Column)} IS {(n.IsNull ? "" : "NOT ")}NULL)";
            case SpoComparisonPredicate c:
            {
                var column = Column(c.Column);
                var name = SqliteSchemaDialect.Quote(column.Name);
                var p = Parameter(SqliteValueConverter.ToStored(column.StorageType, c.Value, table.Name, column.Name));
                return c.Operator switch
                {
                    SpoComparisonOperator.Equal => $"({name} IS {p})",
                    SpoComparisonOperator.NotEqual => $"({name} IS NOT {p})", // true for NULL, as in C#
                    _ => column.IsNullable ? $"COALESCE({name} {Sql(c.Operator)} {p}, 0)" : $"({name} {Sql(c.Operator)} {p})",
                };
            }
            case SpoStringMatchPredicate s:
            {
                var column = Quote(s.Column);
                var p = Parameter(s.Text);
                var test = s.Kind switch
                {
                    SpoStringMatchKind.StartsWith => $"substr({column}, 1, length({p})) = {p}",
                    SpoStringMatchKind.EndsWith => $"substr({column}, length({column}) - length({p}) + 1) = {p}",
                    _ => $"instr({column}, {p}) > 0",
                };
                return $"COALESCE({test}, 0)";
            }
            default:
                throw new ArgumentException(predicate.GetType().Name, nameof(predicate));
        }
    }

    private static string Sql(SpoComparisonOperator op) => op switch
    {
        SpoComparisonOperator.LessThan => "<",
        SpoComparisonOperator.LessThanOrEqual => "<=",
        SpoComparisonOperator.GreaterThan => ">",
        SpoComparisonOperator.GreaterThanOrEqual => ">=",
        _ => throw new ArgumentOutOfRangeException(nameof(op)),
    };

    private string Quote(string column) => SqliteSchemaDialect.Quote(Column(column).Name);

    private SchemaColumn Column(string name) =>
        table.Columns.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal))
        ?? throw new ArgumentException(name, nameof(name));

    private string Parameter(object value)
    {
        var name = "$q" + parameters++;
        command.Parameters.AddWithValue(name, value);
        return name;
    }
}
