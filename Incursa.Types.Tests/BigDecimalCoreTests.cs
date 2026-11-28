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

using System.Data.SqlTypes;
using System.Globalization;
using System.Numerics;
using System.Text.Json;

namespace Incursa.Types.Tests;

public sealed class BigDecimalCoreTests
{
    public static TheoryData<string, string, int, string> IndependentCoreVectors
    {
        get
        {
            var data = new TheoryData<string, string, int, string>();
            string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "bigdecimal-core-vectors.jsonl");
            foreach (string line in File.ReadLines(path))
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                data.Add(
                    root.GetProperty("input").GetString()!,
                    root.GetProperty("coefficient").GetString()!,
                    root.GetProperty("exponent").GetInt32(),
                    root.GetProperty("canonical").GetString()!);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(IndependentCoreVectors))]
    public void Parse_NormalizesPythonGeneratedCoreVectors(
        string input,
        string coefficient,
        int exponent,
        string canonical)
    {
        BigDecimal value = BigDecimal.Parse(input);

        value.Coefficient.ShouldBe(BigInteger.Parse(coefficient, CultureInfo.InvariantCulture));
        value.Exponent.ShouldBe(exponent);
        value.ToString().ShouldBe(canonical);
        BigDecimal.Parse(canonical).ShouldBe(value);
    }

    [Theory]
    [InlineData("1", "1.0")]
    [InlineData("1e2", "100")]
    [InlineData("-0", "0.000")]
    [InlineData("123.4500", "12345e-2")]
    public void EquivalentSpellings_HaveEqualIdentityAndHash(string leftText, string rightText)
    {
        BigDecimal left = BigDecimal.Parse(leftText);
        BigDecimal right = BigDecimal.Parse(rightText);

        left.ShouldBe(right);
        left.GetHashCode().ShouldBe(right.GetHashCode());
        (left == right).ShouldBeTrue();
        left.CompareTo(right).ShouldBe(0);
    }

    [Theory]
    [InlineData("119", "12e1", -1)]
    [InlineData("-119", "-12e1", 1)]
    [InlineData("1e-4096", "9e-4096", -1)]
    [InlineData("99999999999999999999999999999999999999", "1e38", -1)]
    [InlineData("-1e38", "-99999999999999999999999999999999999999", -1)]
    public void CompareTo_OrdersWithoutNativeOrPowerOfTenConversion(
        string leftText,
        string rightText,
        int expectedSign)
    {
        BigDecimal left = BigDecimal.Parse(leftText);
        BigDecimal right = BigDecimal.Parse(rightText);

        Math.Sign(left.CompareTo(right)).ShouldBe(expectedSign);
        Math.Sign(right.CompareTo(left)).ShouldBe(-expectedSign);
    }

    [Theory]
    [InlineData("0", "0")]
    [InlineData("1", "1e0")]
    [InlineData("123.4500", "12345e-2")]
    [InlineData("-79228162514264337593543950335", "-79228162514264337593543950335e0")]
    public void Decimal_RoundTripsExactly(string input, string canonical)
    {
        decimal native = decimal.Parse(input, NumberStyles.Number, CultureInfo.InvariantCulture);
        BigDecimal value = BigDecimal.FromDecimalExact(native);

        value.ToString().ShouldBe(canonical);
        value.ToDecimalExact().ShouldBe(native);
    }

    [Fact]
    public void Decimal_RandomizedBitPatternsRoundTripAndCompareExactly()
    {
        var random = new Random(0x1B1DEC);
        for (int i = 0; i < 1000; i++)
        {
            decimal left = NextDecimal(random);
            decimal right = NextDecimal(random);
            BigDecimal leftValue = BigDecimal.FromDecimalExact(left);
            BigDecimal rightValue = BigDecimal.FromDecimalExact(right);

            leftValue.ToDecimalExact().ShouldBe(left);
            rightValue.ToDecimalExact().ShouldBe(right);
            Math.Sign(leftValue.CompareTo(rightValue)).ShouldBe(Math.Sign(left.CompareTo(right)));
        }
    }

    [Theory]
    [InlineData("2147483647", 2147483647)]
    [InlineData("-2147483648", -2147483648)]
    [InlineData("123000e-3", 123)]
    public void Int32_ExactConversionAcceptsOnlyExactRange(string input, int expected)
    {
        BigDecimal value = BigDecimal.Parse(input);

        value.ToInt32Exact().ShouldBe(expected);
        value.TryToInt32Exact(out int actual).ShouldBeTrue();
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("1e-1")]
    [InlineData("2147483648")]
    [InlineData("-2147483649")]
    public void Int32_ExactConversionRejectsFractionAndRangeLoss(string input)
    {
        BigDecimal value = BigDecimal.Parse(input);

        Should.Throw<OverflowException>(() => value.ToInt32Exact());
        value.TryToInt32Exact(out _).ShouldBeFalse();
    }

    [Fact]
    public void BigIntegerAndInt64ExactConversionsCoverNativeBoundaries()
    {
        BigDecimal maximum = BigDecimal.Parse("9223372036854775807");
        BigDecimal minimum = BigDecimal.Parse("-9223372036854775808");

        maximum.ToInt64Exact().ShouldBe(long.MaxValue);
        minimum.ToInt64Exact().ShouldBe(long.MinValue);
        maximum.TryToInt64Exact(out long maxResult).ShouldBeTrue();
        maxResult.ShouldBe(long.MaxValue);
        BigDecimal.Parse("1e100").ToBigIntegerExact().ShouldBe(BigInteger.Pow(10, 100));
        BigDecimal.Parse("1e-100").TryToBigIntegerExact(out _).ShouldBeFalse();
        Should.Throw<OverflowException>(() => BigDecimal.Parse("9223372036854775808").ToInt64Exact());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("79228162514264337593543950335")]
    [InlineData("-79228162514264337593543950335")]
    [InlineData("1e-28")]
    public void Decimal_ExactConversionMatchesNativeBoundary(string input)
    {
        BigDecimal value = BigDecimal.Parse(input);
        decimal expected = decimal.Parse(input, NumberStyles.Float, CultureInfo.InvariantCulture);

        value.ToDecimalExact().ShouldBe(expected);
        value.TryToDecimalExact(out decimal actual).ShouldBeTrue();
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("79228162514264337593543950336")]
    [InlineData("1e-29")]
    [InlineData("1e29")]
    public void Decimal_ExactConversionRejectsPrecisionOrRangeLoss(string input)
    {
        BigDecimal value = BigDecimal.Parse(input);

        Should.Throw<OverflowException>(() => value.ToDecimalExact());
        value.TryToDecimalExact(out _).ShouldBeFalse();
    }

    [Theory]
    [InlineData("99999999999999999999999999999999999999")]
    [InlineData("0.00000000000000000000000000000000000001")]
    [InlineData("-99999999999999999999999999999999999999")]
    public void Decimal38_ProfileAcceptsMandatorySqlDomain(string input)
    {
        BigDecimal.TryParse(input, BigDecimalProfile.Decimal38, out BigDecimal value).ShouldBeTrue();

        SqlDecimal oracle = SqlDecimal.Parse(input);
        oracle.IsNull.ShouldBeFalse();
        BigDecimal.Parse(oracle.ToString(), BigDecimalProfile.Decimal38).ShouldBe(value);
    }

    [Fact]
    public void ProfilesExposeDocumentedImmutableLimits()
    {
        BigDecimalProfile.Decimal38.MaxCoefficientDigits.ShouldBe(38);
        BigDecimalProfile.Decimal38.MinExponent.ShouldBe(-38);
        BigDecimalProfile.Decimal76.MaxCoefficientDigits.ShouldBe(76);
        BigDecimalProfile.VariableSafe.MaxCoefficientDigits.ShouldBe(4096);
        BigDecimalProfile.VariableSafe.MaxInputLength.ShouldBe(8192);
        BigDecimalProfile.VariableSafe.MaxWorkingDigits.ShouldBe(8192);
        BigDecimalProfile.VariableSafe.MaxTemporaryAllocationBytes.ShouldBe(64 * 1024);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".1")]
    [InlineData("1.")]
    [InlineData("1,000")]
    [InlineData("1_000")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("-Infinity")]
    [InlineData("1e")]
    [InlineData("1e+")]
    [InlineData("1e999999999999999999999999999999999999")]
    [InlineData("10e2147483647")]
    [InlineData("1.0e-2147483648")]
    public void ParseRejectsMalformedOrSpecialValues(string input)
    {
        BigDecimal.TryParse(input, out _).ShouldBeFalse();
        Should.Throw<FormatException>(() => BigDecimal.Parse(input));
    }

    [Fact]
    public void ParseRejectsOversizedInputBeforeConstruction()
    {
        string oversizedInput = new('1', BigDecimalProfile.VariableSafe.MaxInputLength + 1);
        string oversizedCoefficient = new('1', BigDecimalProfile.VariableSafe.MaxCoefficientDigits + 1);

        BigDecimal.TryParse(oversizedInput, out _).ShouldBeFalse();
        BigDecimal.TryParse(oversizedCoefficient, out _).ShouldBeFalse();
    }

    [Fact]
    public void ParseHonorsExplicitTemporaryAllocationBudget()
    {
        var profile = new BigDecimalProfile("TinyBuffer", 8, -8, 8, 8, 8, 2);

        BigDecimal.TryParse("12", profile, out _).ShouldBeFalse();
        BigDecimal.TryParse("1", profile, out BigDecimal value).ShouldBeTrue();
        value.ShouldBe(BigDecimal.One);
    }

    [Fact]
    public void ProfileRejectsNormalizedCoefficientAndExponentOutsideEnvelope()
    {
        BigDecimal.TryParse(new string('9', 39), BigDecimalProfile.Decimal38, out _).ShouldBeFalse();
        BigDecimal.TryParse("1e39", BigDecimalProfile.Decimal38, out _).ShouldBeFalse();
        BigDecimal.TryParse("1e-39", BigDecimalProfile.Decimal38, out _).ShouldBeFalse();
        Should.Throw<ArgumentOutOfRangeException>(
            () => new BigDecimal(BigInteger.Pow(10, 38) + 1, 0, BigDecimalProfile.Decimal38));
    }

    [Fact]
    public void ConstructorDetectsExponentOverflowDuringNormalization()
    {
        Should.Throw<OverflowException>(() => new BigDecimal(10, int.MaxValue));
    }

    [Fact]
    public void FormattingIsCanonicalInvariantAndBounded()
    {
        BigDecimal value = BigDecimal.Parse("-0012300.00e+4");
        Span<char> destination = stackalloc char[32];

        value.ToString("G", CultureInfo.GetCultureInfo("fr-FR")).ShouldBe("-123e6");
        value.TryFormat(destination, out int written, "G", CultureInfo.CurrentCulture).ShouldBeTrue();
        destination[..written].ToString().ShouldBe("-123e6");
        value.TryFormat(destination, out _, "F", null).ShouldBeFalse();
        Should.Throw<FormatException>(() => value.ToString("F", null));
    }

    private static decimal NextDecimal(Random random)
    {
        Span<byte> bytes = stackalloc byte[12];
        random.NextBytes(bytes);
        int low = BitConverter.ToInt32(bytes[..4]);
        int middle = BitConverter.ToInt32(bytes.Slice(4, 4));
        int high = BitConverter.ToInt32(bytes.Slice(8, 4));
        byte scale = (byte)random.Next(0, 29);
        bool negative = random.Next(0, 2) == 1;
        return new decimal(low, middle, high, negative, scale);
    }
}
