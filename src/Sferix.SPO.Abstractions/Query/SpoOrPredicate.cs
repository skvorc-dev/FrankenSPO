namespace Sferix.SPO.Abstractions.Query;

/// <summary>At least one condition holds.</summary>
/// <param name="Left">The first condition.</param>
/// <param name="Right">The second condition.</param>
public sealed record SpoOrPredicate(SpoPredicate Left, SpoPredicate Right) : SpoPredicate;
