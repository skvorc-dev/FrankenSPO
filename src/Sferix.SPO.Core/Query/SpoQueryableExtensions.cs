using System.Linq.Expressions;
using System.Reflection;

namespace Sferix.SPO.Core.Query;

/// <summary>Runs a query asynchronously; implemented by the ObjectSpace's queryable.</summary>
internal interface ISpoListSource<T>
{
    Task<List<T>> ToListAsync(CancellationToken cancellationToken);
}

/// <summary>Extensions for queries returned by <c>ISpoObjectSpace.Query</c>.</summary>
public static class SpoQueryableExtensions
{
    // Safe: IncludeDeleted is a public method declared on this class, so the lookup always finds it.
    internal static readonly MethodInfo IncludeDeletedMethod =
        typeof(SpoQueryableExtensions).GetMethod(nameof(IncludeDeleted))!;

    /// <summary>Includes soft-deleted rows in the results. Entities pending deletion in this ObjectSpace stay out.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">A query from <c>ISpoObjectSpace.Query</c>.</param>
    /// <returns>The query including soft-deleted rows.</returns>
    public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Provider.CreateQuery<T>(Expression.Call(null, IncludeDeletedMethod.MakeGenericMethod(typeof(T)), source.Expression));
    }

    /// <summary>Runs the query and returns the results.</summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="source">A query from <c>ISpoObjectSpace.Query</c>.</param>
    /// <param name="cancellationToken">Cancels the call before it starts.</param>
    /// <returns>The results, tracked by the ObjectSpace.</returns>
    public static Task<List<T>> ToListAsync<T>(this IQueryable<T> source, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        return source is ISpoListSource<T> query
            ? query.ToListAsync(cancellationToken)
            : throw SpoQueryNotSupportedException.ForOperator(nameof(ToListAsync));
    }
}
