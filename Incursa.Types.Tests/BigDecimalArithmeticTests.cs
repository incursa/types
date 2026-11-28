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

public sealed class BigDecimalArithmeticTests
{
    public static TheoryData<string, string, string, string, string, string, string>
        IndependentArithmeticVectors
    {
        get
        {
            var data = new TheoryData<string, string, string, string, string, string, string>();
            string path = Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "bigdecimal-arithmetic-vectors.jsonl");
            foreach (string line in File.ReadLines(path))
            {
                using JsonDocument document = JsonDocument.Parse(line);
                JsonElement root = document.RootElement;
                data.Add(
                    root.GetProperty("left").GetString()!,
                    root.GetProperty("right").GetString()!,
                    root.GetProperty("add").GetString()!,
                    root.GetProperty("subtract").GetString()!,
                    root.GetProperty("multiply").GetString()!,
                    root.GetProperty("divide").GetString()!,
                    root.GetProperty("remainder").GetString()!);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(IndependentArithmeticVectors))]
    public void ExactOperationsMatchIndependentVectors(
        string leftText,
        string rightText,
        string add,
        string subtract,
        string multiply,
        string divide,
        string remainder)
    {
        BigDecimal left = BigDecimal.Parse(leftText);
        BigDecimal right = BigDecimal.Parse(rightText);

        (left + right).ShouldBe(BigDecimal.Parse(add));
        (left - right).ShouldBe(BigDecimal.Parse(subtract));
        (left * right).ShouldBe(BigDecimal.Parse(multiply));
        (left / right).ShouldBe(BigDecimal.Parse(divide));
        (left % right).ShouldBe(BigDecimal.Parse(remainder));
    }

    [Theory]
    [InlineData("1", "2", "5e-1")]
    [InlineData("1", "8", "125e-3")]
    [InlineData("7", "40", "175e-3")]
    [InlineData("-15", "4", "-375e-2")]
    [InlineData("3e10", "6e-5", "5e14")]
    public void DivideExactAcceptsOnlyTerminatingBaseTenResults(
        string left,
        string right,
        string expected)
    {
        BigDecimal.Parse(left).DivideExact(BigDecimal.Parse(right))
            .ShouldBe(BigDecimal.Parse(expected));
    }

    [Theory]
    [InlineData("1", "3")]
    [InlineData("2", "6")]
    [InlineData("22", "7")]
    [InlineData("1e10", "3e-10")]
    public void DivideExactClassifiesNonterminatingResults(string left, string right)
    {
        Should.Throw<BigDecimalNonTerminatingDivisionException>(
            () => BigDecimal.Parse(left) / BigDecimal.Parse(right));
    }

    [Theory]
    [InlineData("7.5", "2", "15e-1")]
    [InlineData("-7.5", "2", "-15e-1")]
    [InlineData("7.5", "-2", "15e-1")]
    [InlineData("-7.5", "-2", "-15e-1")]
    [InlineData("2", "7.5", "2e0")]
    public void RemainderUsesDivisionTruncatedTowardZero(
        string left,
        string right,
        string expected)
    {
        BigDecimal.Parse(left).Remainder(BigDecimal.Parse(right))
            .ShouldBe(BigDecimal.Parse(expected));
    }

    [Fact]
    public void DivisionAndRemainderRejectZeroDivisor()
    {
        BigDecimal value = BigDecimal.Parse("1.25");

        Should.Throw<DivideByZeroException>(() => value.DivideExact(BigDecimal.Zero));
        Should.Throw<DivideByZeroException>(() => value.Remainder(BigDecimal.Zero));
        Should.Throw<DivideByZeroException>(
            () => value.DivideRounded(BigDecimal.Zero, 2, BigDecimalRoundingMode.TowardZero));
    }

    [Theory]
    [InlineData("2.5", 0, BigDecimalRoundingMode.ToNearestTiesToEven, "2e0")]
    [InlineData("3.5", 0, BigDecimalRoundingMode.ToNearestTiesToEven, "4e0")]
    [InlineData("-2.5", 0, BigDecimalRoundingMode.ToNearestTiesToEven, "-2e0")]
    [InlineData("-3.5", 0, BigDecimalRoundingMode.ToNearestTiesToEven, "-4e0")]
    [InlineData("2.5", 0, BigDecimalRoundingMode.ToNearestTiesAwayFromZero, "3e0")]
    [InlineData("-2.5", 0, BigDecimalRoundingMode.ToNearestTiesAwayFromZero, "-3e0")]
    [InlineData("1.21", 1, BigDecimalRoundingMode.TowardZero, "12e-1")]
    [InlineData("1.21", 1, BigDecimalRoundingMode.AwayFromZero, "13e-1")]
    [InlineData("1.21", 1, BigDecimalRoundingMode.TowardPositiveInfinity, "13e-1")]
    [InlineData("1.21", 1, BigDecimalRoundingMode.TowardNegativeInfinity, "12e-1")]
    [InlineData("-1.21", 1, BigDecimalRoundingMode.TowardZero, "-12e-1")]
    [InlineData("-1.21", 1, BigDecimalRoundingMode.AwayFromZero, "-13e-1")]
    [InlineData("-1.21", 1, BigDecimalRoundingMode.TowardPositiveInfinity, "-12e-1")]
    [InlineData("-1.21", 1, BigDecimalRoundingMode.TowardNegativeInfinity, "-13e-1")]
    [InlineData("149", -1, BigDecimalRoundingMode.ToNearestTiesAwayFromZero, "15e1")]
    public void QuantizeImplementsEveryExplicitRoundingMode(
        string input,
        int targetScale,
        BigDecimalRoundingMode mode,
        string expected)
    {
        BigDecimal.Parse(input).Quantize(targetScale, mode)
            .ShouldBe(BigDecimal.Parse(expected));
    }

    [Theory]
    [InlineData("1", "3", 2, BigDecimalRoundingMode.TowardZero, "33e-2")]
    [InlineData("1", "3", 2, BigDecimalRoundingMode.AwayFromZero, "34e-2")]
    [InlineData("1", "3", 2, BigDecimalRoundingMode.TowardPositiveInfinity, "34e-2")]
    [InlineData("1", "3", 2, BigDecimalRoundingMode.TowardNegativeInfinity, "33e-2")]
    [InlineData("-1", "3", 2, BigDecimalRoundingMode.TowardPositiveInfinity, "-33e-2")]
    [InlineData("-1", "3", 2, BigDecimalRoundingMode.TowardNegativeInfinity, "-34e-2")]
    [InlineData("1", "8", 2, BigDecimalRoundingMode.ToNearestTiesToEven, "12e-2")]
    [InlineData("3", "8", 2, BigDecimalRoundingMode.ToNearestTiesToEven, "38e-2")]
    [InlineData("1", "8", 2, BigDecimalRoundingMode.ToNearestTiesAwayFromZero, "13e-2")]
    [InlineData("-1", "8", 2, BigDecimalRoundingMode.ToNearestTiesAwayFromZero, "-13e-2")]
    public void DivideRoundedRequiresScaleAndExplicitMode(
        string left,
        string right,
        int targetScale,
        BigDecimalRoundingMode mode,
        string expected)
    {
        BigDecimal.Parse(left).DivideRounded(BigDecimal.Parse(right), targetScale, mode)
            .ShouldBe(BigDecimal.Parse(expected));
    }

    [Fact]
    public void InvalidRoundingModeAndScaleFailExplicitly()
    {
        BigDecimal value = BigDecimal.One;

        Should.Throw<ArgumentOutOfRangeException>(
            () => value.Quantize(0, (BigDecimalRoundingMode)999));
        Should.Throw<BigDecimalLimitException>(
            () => value.Quantize(39, BigDecimalRoundingMode.TowardZero, BigDecimalContext.Decimal38));
        Should.Throw<BigDecimalLimitException>(
            () => value.DivideRounded(
                BigDecimal.One,
                int.MinValue,
                BigDecimalRoundingMode.TowardZero));
    }

    [Fact]
    public void RoundedZeroRemainsCanonicalWithoutExponentAllocation()
    {
        BigDecimal.Zero.Quantize(4096, BigDecimalRoundingMode.AwayFromZero)
            .ShouldBe(BigDecimal.Zero);
        BigDecimal.Zero.DivideRounded(
                BigDecimal.Parse("1e4096"),
                4096,
                BigDecimalRoundingMode.AwayFromZero)
            .ShouldBe(BigDecimal.Zero);
    }

    [Fact]
    public void ContextClassifiesValueAndWorkingResourceFailures()
    {
        BigDecimal max38 = BigDecimal.Parse(
            "99999999999999999999999999999999999999",
            BigDecimalProfile.Decimal38);
        Should.Throw<BigDecimalLimitException>(
            () => max38.Add(BigDecimal.Parse("2"), BigDecimalContext.Decimal38));

        BigDecimal wideLeft = BigDecimal.Parse("1e4096");
        BigDecimal wideRight = BigDecimal.Parse("1e-4096");
        Should.Throw<BigDecimalLimitException>(() => wideLeft + wideRight);

        var allocationProfile = new BigDecimalProfile("TinyAllocation", 10, -10, 10, 20, 20, 1);
        var allocationContext = new BigDecimalContext(allocationProfile);
        Should.Throw<BigDecimalLimitException>(
            () => BigDecimal.One.Add(BigDecimal.One, allocationContext));
        Should.Throw<BigDecimalLimitException>(
            () => BigDecimal.Parse("1e38", BigDecimalProfile.Decimal38)
                .Multiply(BigDecimal.Parse("1e38", BigDecimalProfile.Decimal38), BigDecimalContext.Decimal38));
    }

    [Fact]
    public void ExactOperatorsNormalizeCrossRepresentationResults()
    {
        BigDecimal sum = BigDecimal.Parse("1.20") + BigDecimal.Parse("2.8000");
        BigDecimal product = BigDecimal.Parse("2.50") * BigDecimal.Parse("4.0");
        BigDecimal quotient = BigDecimal.Parse("10.00") / BigDecimal.Parse("4.000");

        sum.ShouldBe(BigDecimal.Parse("4"));
        product.ShouldBe(BigDecimal.Parse("10"));
        quotient.ShouldBe(BigDecimal.Parse("2.5"));
        sum.GetHashCode().ShouldBe(BigDecimal.Parse("4e0").GetHashCode());
    }

    [Fact]
    public void CompatibleExactOperationsAgreeWithSqlDecimalOracle()
    {
        var random = new Random(0xDEC1A1);
        for (int i = 0; i < 250; i++)
        {
            decimal leftNative = random.Next(-1_000_000, 1_000_001) / 100m;
            decimal rightNative = random.Next(-10_000, 10_001) / 100m;
            BigDecimal left = BigDecimal.FromDecimalExact(leftNative);
            BigDecimal right = BigDecimal.FromDecimalExact(rightNative);
            SqlDecimal leftOracle = new(leftNative);
            SqlDecimal rightOracle = new(rightNative);

            (left + right).ShouldBe(BigDecimal.Parse((leftOracle + rightOracle).ToString()));
            (left - right).ShouldBe(BigDecimal.Parse((leftOracle - rightOracle).ToString()));
            (left * right).ShouldBe(BigDecimal.Parse((leftOracle * rightOracle).ToString()));
        }
    }

    [Fact]
    public void RandomizedNearestQuantizationAgreesWithNativeDecimalOracle()
    {
        var random = new Random(0xA11CE);
        for (int i = 0; i < 1000; i++)
        {
            decimal native = random.Next(-10_000_000, 10_000_001) / 10_000m;
            int scale = random.Next(0, 4);
            BigDecimal value = BigDecimal.FromDecimalExact(native);

            value.Quantize(scale, BigDecimalRoundingMode.ToNearestTiesToEven)
                .ShouldBe(BigDecimal.FromDecimalExact(decimal.Round(native, scale, MidpointRounding.ToEven)));
            value.Quantize(scale, BigDecimalRoundingMode.ToNearestTiesAwayFromZero)
                .ShouldBe(BigDecimal.FromDecimalExact(decimal.Round(native, scale, MidpointRounding.AwayFromZero)));
        }
    }

    [Fact]
    public void ContextsAreImmutableAndOperatorsUseStableDefault()
    {
        BigDecimalContext.Default.Profile.ShouldBe(BigDecimalProfile.VariableSafe);
        BigDecimalContext.Decimal38.Profile.ShouldBe(BigDecimalProfile.Decimal38);
        typeof(BigDecimalContext).GetProperties()
            .ShouldAllBe(property => property.SetMethod == null);
        typeof(BigDecimalProfile).GetProperties()
            .Where(property => !property.GetMethod!.IsStatic)
            .ShouldAllBe(property => property.SetMethod == null);
    }

    [Fact]
    public void UnaryOperatorsPreserveOrNegateExactIdentity()
    {
        BigDecimal value = BigDecimal.Parse("123.45");

        (+value).ShouldBe(value);
        (-value).ShouldBe(BigDecimal.Parse("-123.45"));
        (-BigDecimal.Zero).ShouldBe(BigDecimal.Zero);
    }
}
