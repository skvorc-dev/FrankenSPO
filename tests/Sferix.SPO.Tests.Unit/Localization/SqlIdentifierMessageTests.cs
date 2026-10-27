using System.Globalization;
using Xunit;
using Sferix.SPO.Core.Sql;

namespace Sferix.SPO.Tests.Unit.Localization;

public sealed class SqlIdentifierMessageTests
{
    [Fact]
    public void MessageIsEnglishUnderUnsupportedCulture()
        => AssertMessageIn("fr-FR", "Invalid SQL identifier.");

    [Fact]
    public void MessageIsGermanUnderGermanCulture()
        => AssertMessageIn("de-DE", "Ungültiger SQL-Bezeichner.");

    [Fact]
    public void MessageIsCroatianUnderCroatianCulture()
        => AssertMessageIn("hr-HR", "Neispravan SQL identifikator.");

    [Fact]
    public void MessageIsSpanishUnderSpanishCulture()
        => AssertMessageIn("es-ES", "Identificador SQL no válido.");

    [Theory]
    [InlineData("de")]
    [InlineData("hr")]
    [InlineData("es")]
    public void SatelliteAssemblyIsProducedForEachLanguage(string language)
        => Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, language, "Sferix.SPO.Core.resources.dll")));

    private static void AssertMessageIn(string culture, string expected)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            var ex = Assert.Throws<ArgumentException>(() => new SqlIdentifier("1customer"));
            Assert.StartsWith(expected, ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
