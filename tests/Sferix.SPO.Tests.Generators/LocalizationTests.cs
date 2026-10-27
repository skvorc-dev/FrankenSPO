using System.Globalization;
using Xunit;

namespace Sferix.SPO.Tests.Generators;

// CurrentUICulture is per-thread state; the class runs its tests serially and restores the culture.
public sealed class LocalizationTests
{
    private const string Source =
        "using Sferix.SPO.Abstractions.Metadata;\n[SpoEntity(\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\")] public class Customer { }";

    private const string AbstractSource =
        "using Sferix.SPO.Abstractions.Metadata;\n[SpoEntity(\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\")] public abstract partial class Shape { }";

    private const string InitOnlySource =
        "using Sferix.SPO.Abstractions.Metadata;\n[SpoEntity(\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\")] public partial class C { [SpoField(1)] public int X { get; init; } }";

    private static string MessageIn(string culture, string source = Source)
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            return Assert.Single(GeneratorHarness.Run(source).GeneratorDiagnostics).GetMessage();
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [Fact]
    public void UnsupportedCultureReportsEnglish() =>
        Assert.Equal("Entity 'Customer' and every class containing it must be declared 'partial'", MessageIn("ja-JP"));

    [Theory]
    [InlineData("de", "muss als 'partial' deklariert sein")]
    [InlineData("hr", "moraju biti deklarirani kao 'partial'")]
    [InlineData("es", "deben declararse como 'partial'")]
    public void SupportedCultureReportsTranslatedText(string culture, string expected) =>
        Assert.Contains(expected, MessageIn(culture));

    [Theory]
    [InlineData("de", "abstrakte")]
    [InlineData("hr", "apstraktna")]
    [InlineData("es", "abstracta")]
    public void AbstractEntityMessageIsTranslated(string culture, string expected) =>
        Assert.Contains(expected, MessageIn(culture, AbstractSource));

    [Theory]
    [InlineData("en", "init-only")]
    [InlineData("de", "init")]
    [InlineData("hr", "init")]
    [InlineData("es", "init")]
    public void MissingAccessorMessageNamesInitOnlySetter(string culture, string expected) =>
        Assert.Contains(expected, MessageIn(culture, InitOnlySource));

    [Fact]
    public void FieldNumberReasonIsTranslated()
    {
        var previous = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = new CultureInfo("de");
            var run = GeneratorHarness.Run(
                "using Sferix.SPO.Abstractions.Metadata;\n[SpoEntity(\"3f2504e0-4f89-11d3-9a0c-0305e82c3301\")] public partial class C { [SpoField(0)] public int X { get; set; } }");
            Assert.Contains("positiv", Assert.Single(run.GeneratorDiagnostics).GetMessage());
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }
}
