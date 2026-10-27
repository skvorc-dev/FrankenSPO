namespace Sferix.SPO.Abstractions.Query;

/// <summary>The condition does not hold.</summary>
/// <param name="Operand">The negated condition.</param>
public sealed record SpoNotPredicate(SpoPredicate Operand) : SpoPredicate;
