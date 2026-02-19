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
using System.Globalization;
using System.Text.Json;
using Xunit;

namespace Incursa.Types.Tests;

public class MoneyTests
{
    [Theory]
    [InlineData(123.4567, 123.46)]
    [InlineData(0.999, 1.00)]
    [InlineData(-1.234, -1.23)]
    [InlineData(1.005, 1.00)]
    [InlineData(1.015, 1.02)]
    [InlineData(-2.225, -2.22)]
    public void ToDecimal_NormalizesUsingBankersRounding(decimal input, decimal expected)
    {
        var money = new Money(input);
        Assert.Equal(expected, money.ToDecimal());
    }

    [Theory]
    [InlineData(123.4567, "$123.46")]
    [InlineData(-123.4567, "($123.46)")]
    public void ToAccounting_FormatsCorrectly(decimal input, string expected)
    {
        var money = new Money(input);
        Assert.Equal(expected, money.ToAccounting());
    }

    [Theory]
    [InlineData(1234.567, "$1,234.57")]
    [InlineData(-1234.567, "-$1,234.57")]
    public void ToCurrency_FormatsCorrectly(decimal input, string expected)
    {
        var money = new Money(input);
        Assert.Equal(expected, money.ToCurrency());
    }

    [Fact]
    public void Operators_WorkAsExpected()
    {
        var a = new Money(10.555m);
        var b = new Money(2.333m);
        Assert.Equal(new Money(12.89m), a + b);
        Assert.Equal(new Money(8.23m), a - b);
        Assert.True(a > b);
        Assert.True(b < a);
        Assert.True(a >= b);
        Assert.True(b <= a);
    }

    [Fact]
    public void ComparisonOperators_DistinguishStrictAndInclusive_OnEqualValues()
    {
        var left = new Money(10m);
        var right = new Money(10m);

        Assert.False(left > right);
        Assert.True(left >= right);
        Assert.False(left < right);
        Assert.True(left <= right);
    }

    [Fact]
    public void UnaryOperators_WorkAsExpected()
    {
        var money = new Money(123.45m);
        Assert.Equal(new Money(-123.45m), -money);
        Assert.Equal(new Money(123.45m), +money);
    }

    [Fact]
    public void Multiplication_WithPercentage_Works()
    {
        var money = new Money(200m);
        var percent = new Percentage(0.25m);
        Assert.Equal(new Money(50m), money * percent);
        Assert.Equal(new Money(50m), percent * money);
    }

    [Fact]
    public void Multiplication_WithInteger_Works()
    {
        var money = new Money(200m);
        Assert.Equal(new Money(400m), money * 2);
        Assert.Equal(new Money(400m), 2 * money);
    }

    [Fact]
    public void Parse_AcceptsInvariantNumericForms()
    {
        Assert.Equal(new Money(1234.50m), Money.Parse("1,234.50"));
        Assert.Equal(new Money(123.40m), Money.Parse(" 123.4 "));
        Assert.Equal(new Money(-10.25m), Money.Parse("-10.25"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("notanumber")]
    [InlineData("$10.00")]
    public void TryParse_InvalidValue_ReturnsFalse(string input)
    {
        Assert.False(Money.TryParse(input, out var result));
        Assert.Equal(default, result);
    }

    [Fact]
    public void Parse_InvalidValue_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Money.Parse("not-a-number"));
    }

    [Fact]
    public void JsonConverter_RoundTripsInvariantValue()
    {
        var money = new Money(12.345m);
        var json = JsonSerializer.Serialize(money);

        Assert.Equal("\"12.34\"", json);
        Assert.Equal(money, JsonSerializer.Deserialize<Money>(json));
    }

    [Fact]
    public void TypeConverter_ConvertsToAndFromString()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Money));
        var parsedObject = converter.ConvertFrom("12.50");
        var formattedObject = converter.ConvertTo(new Money(12.50m), typeof(string));
        Assert.NotNull(parsedObject);
        Assert.NotNull(formattedObject);
        var parsed = Assert.IsType<Money>(parsedObject);
        var formatted = Assert.IsType<string>(formattedObject);

        Assert.Equal(new Money(12.50m), parsed);
        Assert.Equal("12.50", formatted);

        var cultureIgnored = converter.ConvertTo(null, new CultureInfo("fr-FR"), new Money(1234.5m), typeof(string));
        Assert.Equal("1,234.50", Assert.IsType<string>(cultureIgnored));
    }

    [Fact]
    public void TypeConverter_ConvertToDecimal_AndUnsupportedType_AreStrict()
    {
        var converter = TypeDescriptor.GetConverter(typeof(Money));
        var money = new Money(12.50m);

        var asDecimal = converter.ConvertTo(money, typeof(decimal));
        Assert.Equal(12.50m, Assert.IsType<decimal>(asDecimal));
        Assert.Throws<NotSupportedException>(() => converter.ConvertTo(money, typeof(int)));
    }

    [Fact]
    public void ToWords_ProducesExpectedOutput()
    {
        var money = new Money(123.45m);
        var words = money.ToWords();
        Assert.Equal("one hundred and twenty-three dollars and forty-five cents", words);
    }

    [Fact]
    public void ToWords_WholeDollarValue_DoesNotEmitCents()
    {
        var words = new Money(123m).ToWords();

        Assert.Contains("dollars", words, StringComparison.Ordinal);
        Assert.DoesNotContain("cents", words, StringComparison.Ordinal);
    }

    [Fact]
    public void CompareTo_ObjectMoney_UsesUnderlyingValue()
    {
        var ten = new Money(10m);

        Assert.Equal(0, ten.CompareTo((object)new Money(10m)));
        Assert.True(ten.CompareTo((object)new Money(11m)) < 0);
        Assert.True(ten.CompareTo((object)new Money(9m)) > 0);
    }

    [Fact]
    public void Unary_Operators_Work()
    {
        var money = new Money(123.45m);
        Assert.Equal(new Money(-123.45m), -money);
        Assert.Equal(new Money(123.45m), +money);
    }
}
