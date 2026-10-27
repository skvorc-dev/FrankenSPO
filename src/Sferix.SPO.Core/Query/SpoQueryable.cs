using System.Collections;
using System.Linq.Expressions;
using Sferix.SPO.Abstractions.Entities;
using Sferix.SPO.Core.ObjectSpace;

namespace Sferix.SPO.Core.Query;

/// <summary>The queryable of one entity type: it only records the expression; translation happens when it runs.</summary>
internal sealed class SpoQueryable<T> : IOrderedQueryable<T>, IQueryProvider, ISpoListSource<T> where T : SPOBaseObject
{
    private readonly SpoObjectSpace space;

    public SpoQueryable(SpoObjectSpace space) : this(space, null) { }

    private SpoQueryable(SpoObjectSpace space, Expression? expression)
    {
        this.space = space;
        Expression = expression ?? Expression.Constant(this);
    }

    public Type ElementType => typeof(T);
    public Expression Expression { get; }
    public IQueryProvider Provider => this;

    public Task<List<T>> ToListAsync(CancellationToken cancellationToken) => space.ExecuteQueryAsync<T>(true, Expression, cancellationToken);

    public IEnumerator<T> GetEnumerator() => space.ExecuteQueryAsync<T>(false, Expression, default).GetAwaiter().GetResult().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) =>
        typeof(TElement) == typeof(T)
            ? (IQueryable<TElement>)(object)new SpoQueryable<T>(space, expression)
            : throw SpoQueryNotSupportedException.ForOperator("Select"); // projections arrive with S-05

    public IQueryable CreateQuery(Expression expression) =>
        expression.Type.IsAssignableTo(typeof(IQueryable<T>))
            ? new SpoQueryable<T>(space, expression)
            : throw SpoQueryNotSupportedException.ForOperator("Select");

    // Scalar operators (First, Count, Any, ...) arrive with S-05c.
    public object Execute(Expression expression) => throw SpoQueryNotSupportedException.ForOperator(OperatorName(expression));

    public TResult Execute<TResult>(Expression expression) => throw SpoQueryNotSupportedException.ForOperator(OperatorName(expression));

    private static string OperatorName(Expression expression) => expression is MethodCallExpression call ? call.Method.Name : expression.NodeType.ToString();
}
