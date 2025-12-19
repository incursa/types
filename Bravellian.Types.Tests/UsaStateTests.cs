using Bravellian;
using Xunit;

namespace Bravellian.Types.Tests;

public class UsaStateTests
{
    [Fact]
    public void AllStates_AreExhaustiveAndCaseInsensitive()
    {
        Assert.Equal(51, UsaState.AllValues.Count);
        Assert.Contains(UsaState.DistrictOfColumbia, UsaState.AllValues);

        foreach (var state in UsaState.AllValues)
        {
            Assert.True(UsaState.TryParse(state.Value, out var parsedUpper));
            Assert.True(UsaState.TryParse(state.Value.ToLowerInvariant(), out var parsedLower));
            Assert.Equal(state, parsedUpper);
            Assert.Equal(state, parsedLower);
        }
    }

    [Fact]
    public void TryParse_RejectsUnknownValues()
    {
        Assert.False(UsaState.TryParse("XX", out _));
        Assert.False(UsaState.TryParse(string.Empty, out _));
        Assert.False(UsaState.TryParse((string?)null, out _));
    }
}
