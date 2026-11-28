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

using System.Globalization;
using System.Numerics;

namespace Incursa;

public readonly partial struct BigDecimal
{
    /// <summary>Returns the exact sum under the stable default context.</summary>
    public BigDecimal Add(BigDecimal other) => Add(other, BigDecimalContext.Default);

    /// <summary>Returns the exact sum under an explicit immutable context.</summary>
    public BigDecimal Add(BigDecimal other, BigDecimalContext context)
    {
        ValidateOperands(this, other, context, "addition");
        int commonExponent = Math.Min(Exponent, other.Exponent);
        int leftShift = CheckedNonNegativeDifference(Exponent, commonExponent, "addition");
        int rightShift = CheckedNonNegativeDifference(other.Exponent, commonExponent, "addition");
        long leftWorkingDigits = (long)GetDigitCount(Coefficient) + leftShift;
        long rightWorkingDigits = (long)GetDigitCount(other.Coefficient) + rightShift;
        EnsureWorkingDigits(Math.Max(leftWorkingDigits, rightWorkingDigits), context, "addition");

        BigInteger left = leftShift == 0
            ? Coefficient
            : Coefficient * Pow10(leftShift, context, leftWorkingDigits, "addition");
        BigInteger right = rightShift == 0
            ? other.Coefficient
            : other.Coefficient * Pow10(rightShift, context, rightWorkingDigits, "addition");
        return CreateResult(left + right, commonExponent, context, "addition");
    }

    /// <summary>Returns the exact difference under the stable default context.</summary>
    public BigDecimal Subtract(BigDecimal other) => Subtract(other, BigDecimalContext.Default);

    /// <summary>Returns the exact difference under an explicit immutable context.</summary>
    public BigDecimal Subtract(BigDecimal other, BigDecimalContext context)
    {
        ValidateOperands(this, other, context, "subtraction");
        return Add(new BigDecimal(-other.Coefficient, other.Exponent, context.Profile), context);
    }

    /// <summary>Returns the exact product under the stable default context.</summary>
    public BigDecimal Multiply(BigDecimal other) => Multiply(other, BigDecimalContext.Default);

    /// <summary>Returns the exact product under an explicit immutable context.</summary>
    public BigDecimal Multiply(BigDecimal other, BigDecimalContext context)
    {
        ValidateOperands(this, other, context, "multiplication");
        long exponent = (long)Exponent + other.Exponent;
        if (exponent is < int.MinValue or > int.MaxValue)
        {
            throw Limit("multiplication", "the result exponent exceeds Int32 bounds");
        }

        long workingDigits = (long)GetDigitCount(Coefficient) + GetDigitCount(other.Coefficient);
        EnsureWorkingDigits(workingDigits, context, "multiplication");
        return CreateResult(Coefficient * other.Coefficient, (int)exponent, context, "multiplication");
    }

    /// <summary>
    /// Returns the exact quotient or throws when its decimal expansion does not terminate.
    /// </summary>
    public BigDecimal DivideExact(BigDecimal divisor) =>
        DivideExact(divisor, BigDecimalContext.Default);

    /// <summary>
    /// Returns the exact quotient under an explicit context or throws when its
    /// decimal expansion does not terminate.
    /// </summary>
    public BigDecimal DivideExact(BigDecimal divisor, BigDecimalContext context)
    {
        ValidateOperands(this, divisor, context, "division");
        if (divisor.Coefficient.IsZero)
        {
            throw new DivideByZeroException();
        }

        if (Coefficient.IsZero)
        {
            return Zero;
        }

        BigInteger greatestCommonDivisor = BigInteger.GreatestCommonDivisor(
            BigInteger.Abs(Coefficient),
            BigInteger.Abs(divisor.Coefficient));
        BigInteger numerator = Coefficient / greatestCommonDivisor;
        BigInteger denominator = divisor.Coefficient / greatestCommonDivisor;
        if (denominator.Sign < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        int powersOfTwo = 0;
        while (denominator.IsEven)
        {
            denominator /= 2;
            powersOfTwo = checked(powersOfTwo + 1);
        }

        int powersOfFive = 0;
        while (BigInteger.Remainder(denominator, 5).IsZero)
        {
            denominator /= 5;
            powersOfFive = checked(powersOfFive + 1);
        }

        if (!denominator.IsOne)
        {
            throw new BigDecimalNonTerminatingDivisionException();
        }

        int decimalPlaces = Math.Max(powersOfTwo, powersOfFive);
        int missingTwos = decimalPlaces - powersOfTwo;
        int missingFives = decimalPlaces - powersOfFive;
        long estimatedDigits = GetDigitCount(numerator);
        estimatedDigits += EstimatePowerDigits(2, missingTwos);
        estimatedDigits += EstimatePowerDigits(5, missingFives);
        EnsureWorkingDigits(estimatedDigits, context, "division");

        if (missingTwos > 0)
        {
            numerator *= BigInteger.Pow(2, missingTwos);
        }

        if (missingFives > 0)
        {
            numerator *= BigInteger.Pow(5, missingFives);
        }

        long resultExponent = (long)Exponent - divisor.Exponent - decimalPlaces;
        if (resultExponent is < int.MinValue or > int.MaxValue)
        {
            throw Limit("division", "the result exponent exceeds Int32 bounds");
        }

        return CreateResult(numerator, (int)resultExponent, context, "division");
    }

    /// <summary>Returns the exact truncated-division residual under the default context.</summary>
    public BigDecimal Remainder(BigDecimal divisor) =>
        Remainder(divisor, BigDecimalContext.Default);

    /// <summary>Returns the exact truncated-division residual under an explicit context.</summary>
    public BigDecimal Remainder(BigDecimal divisor, BigDecimalContext context)
    {
        ValidateOperands(this, divisor, context, "remainder");
        if (divisor.Coefficient.IsZero)
        {
            throw new DivideByZeroException();
        }

        if (Coefficient.IsZero)
        {
            return Zero;
        }

        long exponentDifference = (long)Exponent - divisor.Exponent;
        BigInteger numerator;
        BigInteger denominator;
        int resultExponent;
        if (exponentDifference >= 0)
        {
            int shift = CheckedShift(exponentDifference, "remainder");
            long workingDigits = (long)GetDigitCount(Coefficient) + shift;
            EnsureWorkingDigits(workingDigits, context, "remainder");
            numerator = Coefficient * Pow10(shift, context, workingDigits, "remainder");
            denominator = divisor.Coefficient;
            resultExponent = divisor.Exponent;
        }
        else
        {
            int shift = CheckedShift(-exponentDifference, "remainder");
            long workingDigits = (long)GetDigitCount(divisor.Coefficient) + shift;
            EnsureWorkingDigits(workingDigits, context, "remainder");
            numerator = Coefficient;
            denominator = divisor.Coefficient * Pow10(shift, context, workingDigits, "remainder");
            resultExponent = Exponent;
        }

        BigInteger.DivRem(numerator, denominator, out BigInteger residual);
        return CreateResult(residual, resultExponent, context, "remainder");
    }

    /// <summary>
    /// Divides and rounds to an explicit number of fractional decimal places.
    /// </summary>
    public BigDecimal DivideRounded(
        BigDecimal divisor,
        int targetScale,
        BigDecimalRoundingMode roundingMode) =>
        DivideRounded(divisor, targetScale, roundingMode, BigDecimalContext.Default);

    /// <summary>
    /// Divides and rounds to an explicit number of fractional decimal places
    /// under an explicit immutable context.
    /// </summary>
    public BigDecimal DivideRounded(
        BigDecimal divisor,
        int targetScale,
        BigDecimalRoundingMode roundingMode,
        BigDecimalContext context)
    {
        ValidateOperands(this, divisor, context, "rounded division");
        ValidateRoundingMode(roundingMode);
        if (divisor.Coefficient.IsZero)
        {
            throw new DivideByZeroException();
        }

        int targetExponent = ValidateTargetScale(targetScale, context, "rounded division");
        if (Coefficient.IsZero)
        {
            return Zero;
        }

        long power = (long)Exponent - divisor.Exponent - targetExponent;
        BigInteger numerator = Coefficient;
        BigInteger denominator = divisor.Coefficient;
        if (power >= 0)
        {
            int shift = CheckedShift(power, "rounded division");
            long workingDigits = (long)GetDigitCount(numerator) + shift;
            EnsureWorkingDigits(workingDigits, context, "rounded division");
            numerator *= Pow10(shift, context, workingDigits, "rounded division");
        }
        else
        {
            int shift = CheckedShift(-power, "rounded division");
            long workingDigits = (long)GetDigitCount(denominator) + shift;
            EnsureWorkingDigits(workingDigits, context, "rounded division");
            denominator *= Pow10(shift, context, workingDigits, "rounded division");
        }

        BigInteger rounded = DivideAndRound(numerator, denominator, roundingMode);
        return CreateResult(rounded, targetExponent, context, "rounded division");
    }

    /// <summary>Rounds this value to an explicit number of fractional decimal places.</summary>
    public BigDecimal Quantize(int targetScale, BigDecimalRoundingMode roundingMode) =>
        Quantize(targetScale, roundingMode, BigDecimalContext.Default);

    /// <summary>
    /// Rounds this value to an explicit number of fractional decimal places
    /// under an explicit immutable context.
    /// </summary>
    public BigDecimal Quantize(
        int targetScale,
        BigDecimalRoundingMode roundingMode,
        BigDecimalContext context)
    {
        ValidateOperand(this, context, "quantization");
        ValidateRoundingMode(roundingMode);
        int targetExponent = ValidateTargetScale(targetScale, context, "quantization");
        if (Coefficient.IsZero)
        {
            return Zero;
        }

        long power = (long)Exponent - targetExponent;
        BigInteger rounded;
        if (power >= 0)
        {
            int shift = CheckedShift(power, "quantization");
            long workingDigits = (long)GetDigitCount(Coefficient) + shift;
            EnsureWorkingDigits(workingDigits, context, "quantization");
            rounded = Coefficient * Pow10(shift, context, workingDigits, "quantization");
        }
        else
        {
            int shift = CheckedShift(-power, "quantization");
            long workingDigits = Math.Max(GetDigitCount(Coefficient), (long)shift + 1);
            EnsureWorkingDigits(workingDigits, context, "quantization");
            rounded = DivideAndRound(
                Coefficient,
                Pow10(shift, context, workingDigits, "quantization"),
                roundingMode);
        }

        return CreateResult(rounded, targetExponent, context, "quantization");
    }

    public static BigDecimal operator +(BigDecimal left, BigDecimal right) => left.Add(right);

    public static BigDecimal operator -(BigDecimal left, BigDecimal right) => left.Subtract(right);

    public static BigDecimal operator *(BigDecimal left, BigDecimal right) => left.Multiply(right);

    public static BigDecimal operator /(BigDecimal left, BigDecimal right) => left.DivideExact(right);

    public static BigDecimal operator %(BigDecimal left, BigDecimal right) => left.Remainder(right);

    public static BigDecimal operator +(BigDecimal value) => value;

    public static BigDecimal operator -(BigDecimal value) =>
        CreateResult(-value.Coefficient, value.Exponent, BigDecimalContext.Default, "negation");

    private static BigInteger DivideAndRound(
        BigInteger numerator,
        BigInteger denominator,
        BigDecimalRoundingMode roundingMode)
    {
        if (denominator.Sign < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        BigInteger quotient = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
        if (remainder.IsZero)
        {
            return quotient;
        }

        int direction = numerator.Sign;
        int halfComparison = CompareRemainderToHalf(remainder, denominator);
        bool increment = roundingMode switch
        {
            BigDecimalRoundingMode.TowardZero => false,
            BigDecimalRoundingMode.AwayFromZero => true,
            BigDecimalRoundingMode.TowardPositiveInfinity => direction > 0,
            BigDecimalRoundingMode.TowardNegativeInfinity => direction < 0,
            BigDecimalRoundingMode.ToNearestTiesToEven =>
                halfComparison > 0 || (halfComparison == 0 && !quotient.IsEven),
            BigDecimalRoundingMode.ToNearestTiesAwayFromZero =>
                halfComparison >= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(roundingMode)),
        };

        return increment ? quotient + direction : quotient;
    }

    private static int CompareRemainderToHalf(BigInteger remainder, BigInteger denominator)
    {
        BigInteger half = BigInteger.DivRem(denominator, 2, out BigInteger parity);
        int comparison = BigInteger.Abs(remainder).CompareTo(half);
        return comparison == 0 && !parity.IsZero ? -1 : comparison;
    }

    private static int ValidateTargetScale(
        int targetScale,
        BigDecimalContext context,
        string operation)
    {
        long targetExponent = -(long)targetScale;
        if (targetExponent < context.Profile.MinExponent ||
            targetExponent > context.Profile.MaxExponent)
        {
            throw Limit(operation, $"target scale {targetScale} is outside profile {context.Profile.Name}");
        }

        return (int)targetExponent;
    }

    private static void ValidateRoundingMode(BigDecimalRoundingMode roundingMode)
    {
        if (!Enum.IsDefined(roundingMode))
        {
            throw new ArgumentOutOfRangeException(nameof(roundingMode));
        }
    }

    private static void ValidateOperands(
        BigDecimal left,
        BigDecimal right,
        BigDecimalContext context,
        string operation)
    {
        ValidateOperand(left, context, operation);
        ValidateOperand(right, context, operation);
    }

    private static void ValidateOperand(
        BigDecimal value,
        BigDecimalContext context,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            ValidateValue(value.Coefficient, value.Exponent, context.Profile);
        }
        catch (ArgumentOutOfRangeException exception)
        {
            throw Limit(operation, "an operand is outside the selected profile", exception);
        }
    }

    private static BigDecimal CreateResult(
        BigInteger coefficient,
        int exponent,
        BigDecimalContext context,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            return new BigDecimal(coefficient, exponent, context.Profile);
        }
        catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException)
        {
            throw Limit(operation, "the exact result is outside the selected profile", exception);
        }
    }

    private static int GetDigitCount(BigInteger value) => value.IsZero
        ? 1
        : BigInteger.Abs(value).ToString(CultureInfo.InvariantCulture).Length;

    private static void EnsureWorkingDigits(
        long digits,
        BigDecimalContext context,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (digits < 1 || digits > context.Profile.MaxWorkingDigits)
        {
            throw Limit(
                operation,
                $"estimated working digits {digits} exceed profile " +
                $"{context.Profile.Name}'s {context.Profile.MaxWorkingDigits}-digit limit");
        }

        long estimatedBytes = (digits / 2) + 64;
        if (estimatedBytes > context.Profile.MaxTemporaryAllocationBytes)
        {
            throw Limit(
                operation,
                $"estimated temporary allocation {estimatedBytes} bytes exceeds profile " +
                $"{context.Profile.Name}'s {context.Profile.MaxTemporaryAllocationBytes}-byte limit");
        }
    }

    private static BigInteger Pow10(
        int power,
        BigDecimalContext context,
        long workingDigits,
        string operation)
    {
        EnsureWorkingDigits(workingDigits, context, operation);
        return BigInteger.Pow(10, power);
    }

    private static int CheckedNonNegativeDifference(int left, int right, string operation)
    {
        long difference = (long)left - right;
        return CheckedShift(difference, operation);
    }

    private static int CheckedShift(long shift, string operation)
    {
        if (shift is < 0 or > int.MaxValue)
        {
            throw Limit(operation, "required decimal alignment exceeds Int32 bounds");
        }

        return (int)shift;
    }

    private static long EstimatePowerDigits(int radix, int power)
    {
        if (power == 0)
        {
            return 0;
        }

        return checked((long)Math.Floor(power * Math.Log10(radix)) + 1);
    }

    private static BigDecimalLimitException Limit(string operation, string reason) =>
        new($"BigDecimal {operation} failed because {reason}.");

    private static BigDecimalLimitException Limit(
        string operation,
        string reason,
        Exception innerException) =>
        new($"BigDecimal {operation} failed because {reason}.", innerException);
}
