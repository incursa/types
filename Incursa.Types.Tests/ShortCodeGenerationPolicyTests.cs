using Incursa;

namespace Incursa.Types.Tests;

public class ShortCodeGenerationPolicyTests
{
    [Theory]
    [InlineData("FUCK")]
    [InlineData("23FUCK89")]
    [InlineData("A55X")]
    [InlineData("F4G7")]
    [InlineData("WETBACK")]
    public void IsAcceptable_RejectsDisallowedLiteralAndDigitSubstitutedFragments(string candidate)
    {
        ShortCodeGenerationPolicy.IsAcceptable(candidate).ShouldBeFalse();
    }

    [Theory]
    [InlineData("PUTA", "Spanish")]
    [InlineData("MERDE", "French")]
    [InlineData("SCHWANZ", "German")]
    [InlineData("PUTTANA", "Italian")]
    [InlineData("BUCETA", "Portuguese")]
    [InlineData("SHARMUTA", "Arabic, Latin script")]
    public void IsAcceptable_RejectsCuratedMultilingualFragments(string candidate, string language)
    {
        _ = language;

        ShortCodeGenerationPolicy.IsAcceptable(candidate).ShouldBeFalse();
    }

    [Theory]
    [InlineData("ABCD")]
    [InlineData("2345")]
    [InlineData("HJKMNPQR")]
    [InlineData("UVWXYZ89")]
    public void IsAcceptable_AllowsOrdinaryCandidates(string candidate)
    {
        ShortCodeGenerationPolicy.IsAcceptable(candidate).ShouldBeTrue();
    }

    [Fact]
    public void Parse_PreservesPreviouslyIssuedCodeThatGenerationWouldReject()
    {
        ShortCode parsed = ShortCode.Parse("23-fu-ck-89");

        parsed.RawCode.ShouldBe("23FUCK89");
        ShortCodeGenerationPolicy.IsAcceptable(parsed.RawCode).ShouldBeFalse();
    }

    [Fact]
    public void Generate_AlwaysReturnsAcceptableCandidates()
    {
        for (var i = 0; i < 256; i++)
        {
            ShortCode generated = ShortCode.Generate(8);

            ShortCodeGenerationPolicy.IsAcceptable(generated.RawCode).ShouldBeTrue();
        }
    }
}
