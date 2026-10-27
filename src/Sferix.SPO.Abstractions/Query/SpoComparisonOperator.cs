namespace Sferix.SPO.Abstractions.Query;

/// <summary>A comparison operator of a <see cref="SpoComparisonPredicate"/>.</summary>
public enum SpoComparisonOperator
{
    /// <summary>Equal; a null column value is equal only to a null operand (which the model expresses as <see cref="SpoNullCheckPredicate"/>).</summary>
    Equal,
    /// <summary>Not equal; true for a null column value, as in C#.</summary>
    NotEqual,
    /// <summary>Less than; false for a null column value.</summary>
    LessThan,
    /// <summary>Less than or equal; false for a null column value.</summary>
    LessThanOrEqual,
    /// <summary>Greater than; false for a null column value.</summary>
    GreaterThan,
    /// <summary>Greater than or equal; false for a null column value.</summary>
    GreaterThanOrEqual,
}
