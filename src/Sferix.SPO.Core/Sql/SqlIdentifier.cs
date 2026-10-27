using Sferix.SPO.Core.Resources;

namespace Sferix.SPO.Core.Sql;

/// <summary>Validates provider-neutral unquoted SQL identifiers.</summary>
/// <remarks>Intentionally conservative. This is not a complete SQL compiler or permission check.</remarks>
public readonly record struct SqlIdentifier
{
    public string Value { get; }

    public SqlIdentifier(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value.Length > 63 || !(IsAsciiLetter(value[0]) || value[0] == '_'))
            throw new ArgumentException(CoreMessages.SqlIdentifierInvalid, nameof(value));
        foreach (var ch in value.AsSpan(1))
            if (!(IsAsciiLetter(ch) || char.IsAsciiDigit(ch) || ch == '_'))
                throw new ArgumentException(CoreMessages.SqlIdentifierInvalid, nameof(value));
        Value = value;
    }

    private static bool IsAsciiLetter(char value) => value is >= 'A' and <= 'Z' or >= 'a' and <= 'z';

    public override string ToString() => Value;
}
