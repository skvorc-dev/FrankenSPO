namespace Sferix.SPO.Abstractions.Query;

/// <summary>Matches a text column against a text, ordinal and case-sensitive; the text has no wildcards (D-45).</summary>
/// <param name="Column">The column name.</param>
/// <param name="Kind">The kind of match.</param>
/// <param name="Text">The text; may be empty. An empty text matches every non-null value.</param>
public sealed record SpoStringMatchPredicate(string Column, SpoStringMatchKind Kind, string Text) : SpoPredicate;
