using AgainDj.Domain.Model;

namespace AgainDj.Tests;

public class CamelotKeyTests
{
    [Fact]
    public void Identical_keys_are_compatible()
    {
        Assert.True(new CamelotKey(8, 'A').IsCompatibleWith(new CamelotKey(8, 'A')));
    }

    [Fact]
    public void Adjacent_keys_are_compatible_including_wrap()
    {
        Assert.True(new CamelotKey(8, 'A').IsCompatibleWith(new CamelotKey(9, 'A')));
        Assert.True(new CamelotKey(8, 'A').IsCompatibleWith(new CamelotKey(7, 'A')));
        Assert.True(new CamelotKey(12, 'A').IsCompatibleWith(new CamelotKey(1, 'A')));
    }

    [Fact]
    public void Relative_major_minor_is_compatible()
    {
        Assert.True(new CamelotKey(8, 'A').IsCompatibleWith(new CamelotKey(8, 'B')));
    }

    [Fact]
    public void Distant_keys_are_not_compatible()
    {
        Assert.False(new CamelotKey(8, 'A').IsCompatibleWith(new CamelotKey(2, 'A')));
    }

    [Theory]
    [InlineData("8A", 8, 'A')]
    [InlineData("12B", 12, 'B')]
    public void Parse_reads_number_and_letter(string text, int number, char letter)
    {
        var key = CamelotKey.Parse(text);
        Assert.Equal(number, key.Number);
        Assert.Equal(letter, key.Letter);
    }
}
