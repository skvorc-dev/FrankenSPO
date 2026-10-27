using System.Linq.Expressions;
using Sferix.SPO.Abstractions.Query;
using Sferix.SPO.Abstractions.Schema;
using Sferix.SPO.Core.ObjectSpace;

namespace Sferix.SPO.Core.Query;

/// <summary>Turns the recorded expression into a neutral <see cref="SpoQuery"/> (D-32, D-45, D-46).</summary>
/// <remarks>
/// Columns come from the entity's metadata only; captured variables are evaluated to values that become parameters.
/// Anything outside the subset throws <see cref="SpoQueryNotSupportedException"/>. Null comparisons follow C# semantics.
/// </remarks>
internal sealed class QueryTranslator(EntityInfo info)
{
    private SpoPredicate? where;
    private readonly List<SpoOrdering> ordering = [];
    private int skip;
    private int? take;
    private bool includeDeleted;
    private bool paged;

    public static SpoQuery Translate(Expression expression, EntityInfo info)
    {
        var translator = new QueryTranslator(info);
        translator.Visit(expression);
        return translator.Build();
    }

    private SpoQuery Build()
    {
        var predicate = where;
        if (!includeDeleted)
        {
            var notDeleted = new SpoComparisonPredicate(info.Table.Columns[info.IsDeletedColumn].Name, SpoComparisonOperator.Equal, false);
            predicate = predicate is null ? notDeleted : new SpoAndPredicate(predicate, notDeleted);
        }

        // The primary key as the last key makes Skip/Take and the result order deterministic.
        var key = info.Table.PrimaryKey[0];
        var keys = ordering.Any(o => o.Column == key) ? ordering : [.. ordering, new SpoOrdering(key, false)];
        return new SpoQuery(info.Table, predicate, keys, skip > 0 ? skip : null, take);
    }

    private void Visit(Expression expression)
    {
        switch (expression)
        {
            case ConstantExpression:
                return;
            case MethodCallExpression call when call.Arguments.Count > 0:
                Visit(call.Arguments[0]);
                Apply(call);
                return;
            default:
                throw SpoQueryNotSupportedException.ForOperator(expression.NodeType.ToString());
        }
    }

    private void Apply(MethodCallExpression call)
    {
        var name = call.Method.Name;
        var declaring = call.Method.DeclaringType;
        if (declaring == typeof(SpoQueryableExtensions) && name == nameof(SpoQueryableExtensions.IncludeDeleted))
        {
            includeDeleted = true;
            return;
        }
        if (declaring != typeof(Queryable))
            throw SpoQueryNotSupportedException.ForOperator(name);

        switch (name)
        {
            case nameof(Queryable.Where) when !paged:
                var predicate = Predicate(Selector(call, 1, 1), Selector(call, 1, 1).Parameters[0]);
                where = where is null ? predicate : new SpoAndPredicate(where, predicate);
                return;
            case nameof(Queryable.OrderBy) when !paged:
            case nameof(Queryable.OrderByDescending) when !paged:
                ordering.Clear();
                ordering.Add(Order(call, name.EndsWith("Descending", StringComparison.Ordinal)));
                return;
            case nameof(Queryable.ThenBy) when !paged:
            case nameof(Queryable.ThenByDescending) when !paged:
                ordering.Add(Order(call, name.EndsWith("Descending", StringComparison.Ordinal)));
                return;
            case nameof(Queryable.Skip) when take is null:
                skip = checked(skip + Math.Max(0, Count(call.Arguments[1])));
                paged = true;
                return;
            case nameof(Queryable.Take):
                var wanted = Math.Max(0, Count(call.Arguments[1]));
                take = take is { } current ? Math.Min(current, wanted) : wanted;
                paged = true;
                return;
            default:
                // Includes Where/OrderBy after Skip/Take: SQL would filter before paging, LINQ after.
                throw SpoQueryNotSupportedException.ForOperator(name);
        }
    }

    private static LambdaExpression Selector(MethodCallExpression call, int argument, int parameters)
    {
        var lambda = (call.Arguments[argument] as UnaryExpression)?.Operand as LambdaExpression;
        return lambda is { } && lambda.Parameters.Count == parameters ? lambda : throw SpoQueryNotSupportedException.ForOperator(call.Method.Name);
    }

    // Safe: Skip and Take take an int argument, so the evaluated value is a boxed int, never null.
    private static int Count(Expression expression) => (int)Evaluate(expression)!;

    private SpoOrdering Order(MethodCallExpression call, bool descending)
    {
        var lambda = Selector(call, 1, 1);
        var column = Column(lambda.Body, lambda.Parameters[0]) ?? throw SpoQueryNotSupportedException.ForOperator(call.Method.Name);
        var schemaColumn = info.Table.Columns[column.Index];
        // byte[] has no order in C#; decimal and DateTimeOffset are TEXT in storage (D-44, D-46).
        if (schemaColumn.StorageType is SpoStorageType.Decimal or SpoStorageType.DateTimeOffset or SpoStorageType.Binary)
            throw SpoQueryNotSupportedException.ForColumn(schemaColumn.Name, TypeName(schemaColumn.StorageType));
        return new SpoOrdering(schemaColumn.Name, descending);
    }

    private SpoPredicate Predicate(LambdaExpression lambda, ParameterExpression x) => Predicate(lambda.Body, x);

    private SpoPredicate Predicate(Expression e, ParameterExpression x)
    {
        switch (e)
        {
            case BinaryExpression { NodeType: ExpressionType.AndAlso or ExpressionType.And } b:
                return new SpoAndPredicate(Predicate(b.Left, x), Predicate(b.Right, x));
            case BinaryExpression { NodeType: ExpressionType.OrElse or ExpressionType.Or } b:
                return new SpoOrPredicate(Predicate(b.Left, x), Predicate(b.Right, x));
            case UnaryExpression { NodeType: ExpressionType.Not } u:
                return new SpoNotPredicate(Predicate(u.Operand, x));
            case BinaryExpression { NodeType: ExpressionType.Equal or ExpressionType.NotEqual or ExpressionType.LessThan
                or ExpressionType.LessThanOrEqual or ExpressionType.GreaterThan or ExpressionType.GreaterThanOrEqual } b:
                return Comparison(b, x);
            case MethodCallExpression { Method.DeclaringType: var type } call when type == typeof(string):
                return StringMatch(call, x);
            case MemberExpression { Member.Name: "HasValue" } m when Column(m.Expression!, x) is { } column:
                return new SpoNullCheckPredicate(info.Table.Columns[column.Index].Name, false);
            case var _ when !Uses(e, x) && e.Type == typeof(bool):
                // Safe: the guard above checked e.Type == typeof(bool) and a non-nullable bool is never null.
                return new SpoConstantPredicate((bool)Evaluate(e)!);
            case var _ when Column(e, x) is { } column && info.Table.Columns[column.Index].StorageType == SpoStorageType.Boolean:
                return new SpoComparisonPredicate(info.Table.Columns[column.Index].Name, SpoComparisonOperator.Equal, true);
            default:
                throw SpoQueryNotSupportedException.ForOperator(e.NodeType.ToString());
        }
    }

    private SpoPredicate Comparison(BinaryExpression b, ParameterExpression x)
    {
        var left = Column(b.Left, x);
        var right = Column(b.Right, x);
        if (left is not null && right is not null)
            throw SpoQueryNotSupportedException.ForOperator("column comparison");
        if (left is null && right is null)
        {
            if (Uses(b, x))
                throw SpoQueryNotSupportedException.ForOperator(b.NodeType.ToString());
            // Safe: a comparison node has type bool (not bool?) here because neither side is a column, so the value is never null.
            return new SpoConstantPredicate((bool)Evaluate(b)!);
        }

        var column = (left ?? right)!; // Safe: the checks above ruled out both being null.
        var operand = left is not null ? b.Right : b.Left;
        if (Uses(operand, x))
            throw SpoQueryNotSupportedException.ForOperator(b.NodeType.ToString());
        var op = Operator(b.NodeType, flip: left is null);

        var schemaColumn = info.Table.Columns[column.Index];
        // decimal and DateTimeOffset are TEXT in storage; byte[] equality in C# is by reference.
        if (schemaColumn.StorageType is SpoStorageType.Decimal or SpoStorageType.DateTimeOffset or SpoStorageType.Binary)
            throw SpoQueryNotSupportedException.ForColumn(schemaColumn.Name, TypeName(schemaColumn.StorageType));

        var value = Coerce(schemaColumn.StorageType, Evaluate(operand), column.Property.EnumUnderlyingType is not null);
        if (value is null)
            return op switch
            {
                SpoComparisonOperator.Equal => new SpoNullCheckPredicate(schemaColumn.Name, true),
                SpoComparisonOperator.NotEqual => new SpoNullCheckPredicate(schemaColumn.Name, false),
                _ => new SpoConstantPredicate(false), // in C# an ordering comparison with null is false
            };
        return new SpoComparisonPredicate(schemaColumn.Name, op, value);
    }

    private SpoPredicate StringMatch(MethodCallExpression call, ParameterExpression x)
    {
        var kind = call.Method.Name switch
        {
            nameof(string.StartsWith) => SpoStringMatchKind.StartsWith,
            nameof(string.EndsWith) => SpoStringMatchKind.EndsWith,
            nameof(string.Contains) => SpoStringMatchKind.Contains,
            _ => throw SpoQueryNotSupportedException.ForOperator(call.Method.Name),
        };
        var parameters = call.Method.GetParameters();
        // Only the ordinal forms: the one-argument StartsWith/EndsWith are culture-sensitive in .NET, which SQL cannot match (D-45).
        var supported = call.Object is not null && parameters.Length >= 1 && parameters[0].ParameterType == typeof(string)
            && (parameters.Length == 1 || (parameters.Length == 2 && parameters[1].ParameterType == typeof(StringComparison)
                && Evaluate(call.Arguments[1]) is StringComparison.Ordinal));
        var column = supported ? Column(call.Object!, x) : null;
        if (column is null || Uses(call.Arguments[0], x))
            throw SpoQueryNotSupportedException.ForOperator(call.Method.Name);
        var schemaColumn = info.Table.Columns[column.Index];
        if (schemaColumn.StorageType != SpoStorageType.String)
            throw SpoQueryNotSupportedException.ForOperator(call.Method.Name);
        var text = Evaluate(call.Arguments[0]) as string ?? throw SpoQueryNotSupportedException.ForOperator(call.Method.Name);
        return new SpoStringMatchPredicate(schemaColumn.Name, kind, text);
    }

    private sealed record ColumnRef(int Index, Abstractions.Metadata.PropertyMetadata Property);

    /// <summary>The column behind <c>x.Prop</c> (also through conversions and <c>.Value</c> of a nullable); null if it is not one.</summary>
    private ColumnRef? Column(Expression e, ParameterExpression x)
    {
        while (e is UnaryExpression { NodeType: ExpressionType.Convert or ExpressionType.ConvertChecked or ExpressionType.TypeAs } u)
            e = u.Operand;
        if (e is MemberExpression { Member.Name: "Value", Expression: { } inner } && Nullable.GetUnderlyingType(inner.Type) is not null)
            e = inner;
        if (e is not MemberExpression { Expression: var owner, Member: var member } || owner != x)
            return null;
        var index = info.FindColumn(member.Name);
        return index >= 0 ? new ColumnRef(index, info.PropertyAt(index)) : throw SpoQueryNotSupportedException.ForOperator(member.Name);
    }

    private static object? Coerce(SpoStorageType storage, object? value, bool isEnum)
    {
        if (value is null)
            return null;
        return storage switch
        {
            SpoStorageType.Int32 => Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture),
            SpoStorageType.Int64 => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture),
            SpoStorageType.Double => Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture),
            _ => isEnum ? throw SpoQueryNotSupportedException.ForOperator("enum comparison") : value,
        };
    }

    private static SpoComparisonOperator Operator(ExpressionType type, bool flip) => (type, flip) switch
    {
        (ExpressionType.Equal, _) => SpoComparisonOperator.Equal,
        (ExpressionType.NotEqual, _) => SpoComparisonOperator.NotEqual,
        (ExpressionType.LessThan, false) or (ExpressionType.GreaterThan, true) => SpoComparisonOperator.LessThan,
        (ExpressionType.LessThanOrEqual, false) or (ExpressionType.GreaterThanOrEqual, true) => SpoComparisonOperator.LessThanOrEqual,
        (ExpressionType.GreaterThan, false) or (ExpressionType.LessThan, true) => SpoComparisonOperator.GreaterThan,
        _ => SpoComparisonOperator.GreaterThanOrEqual,
    };

    private static string TypeName(SpoStorageType type) => type switch
    {
        SpoStorageType.Decimal => "decimal",
        SpoStorageType.DateTimeOffset => "DateTimeOffset",
        _ => "byte[]",
    };

    private static bool Uses(Expression e, ParameterExpression x)
    {
        var finder = new ParameterFinder(x);
        finder.Visit(e);
        return finder.Found;
    }

    /// <summary>Evaluates a part of the query that does not touch the entity: a constant or a captured variable.</summary>
    private static object? Evaluate(Expression e)
    {
        if (e is ConstantExpression constant)
            return constant.Value;
        try
        {
            return Expression.Lambda(e).Compile().DynamicInvoke();
        }
        catch (System.Reflection.TargetInvocationException ex) when (ex.InnerException is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    private sealed class ParameterFinder(ParameterExpression x) : ExpressionVisitor
    {
        public bool Found { get; private set; }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Found |= node == x;
            return node;
        }
    }
}
