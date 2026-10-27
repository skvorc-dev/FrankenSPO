namespace Sferix.SPO.Abstractions.Query;

/// <summary>A condition that is always true or always false.</summary>
/// <param name="Value">The constant.</param>
public sealed record SpoConstantPredicate(bool Value) : SpoPredicate;
