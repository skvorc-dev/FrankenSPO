namespace Sferix.SPO.Abstractions.Query;

/// <summary>Both conditions hold.</summary>
/// <param name="Left">The first condition.</param>
/// <param name="Right">The second condition.</param>
public sealed record SpoAndPredicate(SpoPredicate Left, SpoPredicate Right) : SpoPredicate;
