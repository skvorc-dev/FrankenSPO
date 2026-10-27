namespace Sferix.SPO.Abstractions.Query;

/// <summary>How a <see cref="SpoStringMatchPredicate"/> matches: always ordinal and case-sensitive (D-45).</summary>
public enum SpoStringMatchKind
{
    /// <summary>The column starts with the text.</summary>
    StartsWith,
    /// <summary>The column ends with the text.</summary>
    EndsWith,
    /// <summary>The column contains the text.</summary>
    Contains,
}
