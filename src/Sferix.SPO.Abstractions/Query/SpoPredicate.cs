namespace Sferix.SPO.Abstractions.Query;

/// <summary>A provider-neutral condition. Every predicate is two-valued: it is true or false, never unknown (D-46).</summary>
/// <remarks>The set of subclasses is closed. A provider renders them with parameters only; column names come from the schema.</remarks>
public abstract record SpoPredicate
{
    private protected SpoPredicate()
    {
    }
}
