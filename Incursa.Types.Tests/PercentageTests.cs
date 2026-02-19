// Copyright (c) Samuel McAravey
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Incursa;
using System.ComponentModel;
using Xunit;

namespace Incursa.Types.Tests;

public class PercentageTests
{
    [Theory]
    [InlineData(0.123456, 0.1234)]
    [InlineData(0.99999, 0.9999)]
    [InlineData(-0.123456, -0.1234)]
    public void Value_TruncatesToFourDecimalPlaces(decimal input, decimal expected)
    {
        var pct = new Percentage(input);
        Assert.Equal(expected, pct.Value);
    }

    [Theory]
    [InlineData(0.123456, 12.34)]
    [InlineData(0.99999, 99.99)]
    [InlineData(-0.123456, -12.34)]
    public void ScaledValue_TruncatesAndScales(decimal input, decimal expected)
    {
        var pct = new Percentage(input);
        Assert.Equal(expected, pct.ScaledValue);
    }

    [Theory]
    [InlineData(0.1234, "12.34%")]
    [InlineData(0.5, "50.00%")]
    public void ToString_DefaultFormats(decimal input, string expected)
    {
        var pct = new Percentage(input);
        Assert.Equal(expected, pct.ToString());
    }

    [Theory]
    [InlineData(0.123456, "12.34%")]
    [InlineData(1.0, "100.00%")]
    public void ToStringRaw_FormatsCorrectly(decimal input, string expected)
    {
        var pct = new Percentage(input);
        Assert.Equal(expected, pct.ToStringRaw());
    }

    [Fact]
    public void Operators_WorkAsExpected()
    {
        var a = new Percentage(0.5m);
        var b = new Percentage(0.25m);
        Assert.True(a > b);
        Assert.True(b < a);
        Assert.True(a >= b);
        Assert.True(b <= a);
    }

    [Fact]
    public void Operators_WithEqualValues_AreStrict()
    {
        var a = new Percentage(0.25m);
        var b = new Percentage(0.25m);

        Assert.False(a > b);
        Assert.False(a < b);
        Assert.True(a >= b);
        Assert.True(a <= b);
    }

    [Fact]
    public void ToString_WithZeroDecimals_UsesIntegerPercentFormat()
    {
        var pct = new Percentage(0.1234m);

        Assert.Equal("12%", pct.ToString(0));
    }

    [Fact]
    public void Parse_And_TryParse_Work()
    {
        var parsed = Percentage.Parse("0.25");
        Assert.Equal(new Percentage(0.25m), parsed);
        Assert.True(Percentage.TryParse("0.25", out var result));
        Assert.Equal(new Percentage(0.25m), result);
        Assert.Null(Percentage.TryParse("notanumber"));
    }

    [Fact]
    public void TryParse_NullableOverload_ReturnsValueForValidInput()
    {
        var parsed = Percentage.TryParse("0.25");

        Assert.NotNull(parsed);
        Assert.Equal(new Percentage(0.25m), parsed.Value);
    }

    [Fact]
    public void CompareTo_Overloads_AreDeterministic()
    {
        var p = new Percentage(0.25m);

        Assert.Equal(0, p.CompareTo(new Percentage(0.25m)));
        Assert.True(p.CompareTo(new Percentage(0.30m)) < 0);
        Assert.True(p.CompareTo(new Percentage(0.20m)) > 0);

        Assert.Equal(0, p.CompareTo((object)new Percentage(0.25m)));
        Assert.Throws<ArgumentException>(() => p.CompareTo("not-a-percentage"));
    }

    [Fact]
    public void TypeConverter_ConvertTo_UnsupportedDestination_Throws()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Percentage));

        Assert.Throws<NotSupportedException>(() => converter.ConvertTo(new Percentage(0.25m), typeof(int)));
    }
}
