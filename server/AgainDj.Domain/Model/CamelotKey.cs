namespace AgainDj.Domain.Model;

/// <summary>
/// A key on the Camelot wheel used for harmonic mixing. <see cref="Number"/> is
/// 1..12 and <see cref="Letter"/> is 'A' (minor) or 'B' (major).
/// </summary>
public readonly record struct CamelotKey(int Number, char Letter)
{
    public bool IsValid => Number is >= 1 and <= 12 && Letter is 'A' or 'B';

    /// <summary>
    /// Harmonically compatible when the keys are identical, relative
    /// major/minor (same number), or adjacent on the wheel (±1, wrapping 12↔1).
    /// </summary>
    public bool IsCompatibleWith(CamelotKey other)
    {
        if (!IsValid || !other.IsValid)
        {
            return false;
        }

        if (Number == other.Number)
        {
            return true; // identical or relative major/minor
        }

        if (Letter == other.Letter)
        {
            var diff = Math.Abs(Number - other.Number);
            return diff is 1 or 11; // adjacent, with 12↔1 wrap
        }

        return false;
    }

    public override string ToString() => IsValid ? $"{Number}{Letter}" : "?";

    public static CamelotKey Parse(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length >= 2)
        {
            var letter = char.ToUpperInvariant(value[^1]);
            if (int.TryParse(value[..^1], out var number))
            {
                return new CamelotKey(number, letter);
            }
        }

        return new CamelotKey(0, '?');
    }
}
