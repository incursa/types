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

/// <summary>
/// Represents an exact finite decimal as a normalized integer coefficient
/// multiplied by ten raised to an integral exponent.
/// </summary>
public readonly struct BigDecimal
    : IComparable,
      IComparable<BigDecimal>,
      IEquatable<BigDecimal>,
      ISpanFormattable,
      ISpanParsable<BigDecimal>
{
    private static readonly BigInteger DecimalMaxCoefficient = BigInteger.Parse(
        "79228162514264337593543950335",
        CultureInfo.InvariantCulture);

    /// <summary>Represents exact zero.</summary>
    public static readonly BigDecimal Zero = default;

    /// <summary>Represents exact one.</summary>
    public static readonly BigDecimal One = new(BigInteger.One, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="BigDecimal"/> struct using
    /// the <see cref="BigDecimalProfile.VariableSafe"/> limits.
    /// </summary>
    public BigDecimal(BigInteger coefficient, int exponent)
        : this(coefficient, exponent, BigDecimalProfile.VariableSafe)
    {
    }

    /// <summary>Initializes a new normalized value under <paramref name="profile"/>.</summary>
    public BigDecimal(BigInteger coefficient, int exponent, BigDecimalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Normalize(ref coefficient, ref exponent);
        ValidateValue(coefficient, exponent, profile);
        Coefficient = coefficient;
        Exponent = exponent;
    }

    /// <summary>Gets the normalized signed integer coefficient.</summary>
    public BigInteger Coefficient { get; }

    /// <summary>Gets the normalized power-of-ten exponent.</summary>
    public int Exponent { get; }

    /// <summary>Creates an exact value from a native decimal.</summary>
    public static BigDecimal FromDecimalExact(decimal value)
    {
        int[] bits = decimal.GetBits(value);
        BigInteger coefficient = (uint)bits[0];
        coefficient |= (BigInteger)(uint)bits[1] << 32;
        coefficient |= (BigInteger)(uint)bits[2] << 64;

        if ((bits[3] & int.MinValue) != 0)
        {
            coefficient = -coefficient;
        }

        int scale = (bits[3] >> 16) & 0x7F;
        return new BigDecimal(coefficient, -scale);
    }

    /// <summary>Parses invariant exact-decimal text with default limits.</summary>
    public static BigDecimal Parse(string s) => Parse(s, BigDecimalProfile.VariableSafe);

    /// <summary>Parses invariant exact-decimal text with explicit limits.</summary>
    public static BigDecimal Parse(string s, BigDecimalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(s);
        return Parse(s.AsSpan(), profile);
    }

    /// <summary>Parses invariant exact-decimal text with explicit limits.</summary>
    public static BigDecimal Parse(ReadOnlySpan<char> s, BigDecimalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (!TryParseCore(s, profile, out BigDecimal result))
        {
            throw new FormatException("The value is not valid bounded invariant BigDecimal text.");
        }

        return result;
    }

    /// <inheritdoc/>
    public static BigDecimal Parse(string s, IFormatProvider? provider) => Parse(s);

    /// <inheritdoc/>
    public static BigDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider) =>
        Parse(s, BigDecimalProfile.VariableSafe);

    /// <summary>Attempts to parse invariant exact-decimal text with default limits.</summary>
    public static bool TryParse([NotNullWhen(true)] string? s, out BigDecimal result) =>
        TryParse(s, BigDecimalProfile.VariableSafe, out result);

    /// <summary>Attempts to parse invariant exact-decimal text with explicit limits.</summary>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        BigDecimalProfile profile,
        out BigDecimal result)
    {
        ArgumentNullException.ThrowIfNull(profile);
        if (s is null)
        {
            result = default;
            return false;
        }

        return TryParseCore(s.AsSpan(), profile, out result);
    }

    /// <summary>Attempts to parse invariant exact-decimal text with explicit limits.</summary>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        BigDecimalProfile profile,
        out BigDecimal result)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return TryParseCore(s, profile, out result);
    }

    /// <inheritdoc/>
    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        out BigDecimal result) => TryParse(s, out result);

    /// <inheritdoc/>
    public static bool TryParse(
        ReadOnlySpan<char> s,
        IFormatProvider? provider,
        out BigDecimal result) => TryParseCore(s, BigDecimalProfile.VariableSafe, out result);

    /// <summary>Returns the exact integer value or throws if this value is fractional.</summary>
    public BigInteger ToBigIntegerExact()
    {
        if (Coefficient.IsZero)
        {
            return BigInteger.Zero;
        }

        if (Exponent >= 0)
        {
            return Coefficient * BigInteger.Pow(10, Exponent);
        }

        BigInteger divisor = BigInteger.Pow(10, checked(-Exponent));
        BigInteger quotient = BigInteger.DivRem(Coefficient, divisor, out BigInteger remainder);
        return remainder.IsZero
            ? quotient
            : throw new OverflowException("The BigDecimal value has a fractional component.");
    }

    /// <summary>Attempts to return the exact integer value.</summary>
    public bool TryToBigIntegerExact(out BigInteger result)
    {
        try
        {
            result = ToBigIntegerExact();
            return true;
        }
        catch (OverflowException)
        {
            result = default;
            return false;
        }
    }

    /// <summary>Returns the exact 32-bit integer value.</summary>
    public int ToInt32Exact() => checked((int)ToBigIntegerExact());

    /// <summary>Attempts to return the exact 32-bit integer value.</summary>
    public bool TryToInt32Exact(out int result)
    {
        if (TryToBigIntegerExact(out BigInteger integer) &&
            integer >= int.MinValue &&
            integer <= int.MaxValue)
        {
            result = (int)integer;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>Returns the exact 64-bit integer value.</summary>
    public long ToInt64Exact() => checked((long)ToBigIntegerExact());

    /// <summary>Attempts to return the exact 64-bit integer value.</summary>
    public bool TryToInt64Exact(out long result)
    {
        if (TryToBigIntegerExact(out BigInteger integer) &&
            integer >= long.MinValue &&
            integer <= long.MaxValue)
        {
            result = (long)integer;
            return true;
        }

        result = default;
        return false;
    }

    /// <summary>Returns the exact native decimal value.</summary>
    public decimal ToDecimalExact()
    {
        if (Coefficient.IsZero)
        {
            return decimal.Zero;
        }

        BigInteger coefficient = Coefficient;
        int scale;
        if (Exponent > 0)
        {
            coefficient *= BigInteger.Pow(10, Exponent);
            scale = 0;
        }
        else
        {
            scale = checked(-Exponent);
            if (scale > 28)
            {
                throw new OverflowException("The BigDecimal scale exceeds native decimal precision.");
            }
        }

        bool negative = coefficient.Sign < 0;
        BigInteger magnitude = BigInteger.Abs(coefficient);
        if (magnitude > DecimalMaxCoefficient)
        {
            throw new OverflowException("The BigDecimal magnitude exceeds the native decimal range.");
        }

        uint low = (uint)(magnitude & uint.MaxValue);
        uint middle = (uint)((magnitude >> 32) & uint.MaxValue);
        uint high = (uint)((magnitude >> 64) & uint.MaxValue);
        return new decimal((int)low, (int)middle, (int)high, negative, (byte)scale);
    }

    /// <summary>Attempts to return the exact native decimal value.</summary>
    public bool TryToDecimalExact(out decimal result)
    {
        try
        {
            result = ToDecimalExact();
            return true;
        }
        catch (OverflowException)
        {
            result = default;
            return false;
        }
    }

    /// <inheritdoc/>
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return obj is BigDecimal other
            ? CompareTo(other)
            : throw new ArgumentException($"Object must be of type {nameof(BigDecimal)}.", nameof(obj));
    }

    /// <inheritdoc/>
    public int CompareTo(BigDecimal other)
    {
        int signComparison = Coefficient.Sign.CompareTo(other.Coefficient.Sign);
        if (signComparison != 0)
        {
            return signComparison;
        }

        if (Coefficient.IsZero)
        {
            return 0;
        }

        string leftDigits = BigInteger.Abs(Coefficient).ToString(CultureInfo.InvariantCulture);
        string rightDigits = BigInteger.Abs(other.Coefficient).ToString(CultureInfo.InvariantCulture);
        long leftMagnitude = (long)leftDigits.Length + Exponent;
        long rightMagnitude = (long)rightDigits.Length + other.Exponent;
        int magnitudeComparison = leftMagnitude.CompareTo(rightMagnitude);
        if (magnitudeComparison == 0)
        {
            int length = Math.Max(leftDigits.Length, rightDigits.Length);
            for (int i = 0; i < length; i++)
            {
                char left = i < leftDigits.Length ? leftDigits[i] : '0';
                char right = i < rightDigits.Length ? rightDigits[i] : '0';
                if (left != right)
                {
                    magnitudeComparison = left.CompareTo(right);
                    break;
                }
            }
        }

        return Coefficient.Sign > 0 ? magnitudeComparison : -magnitudeComparison;
    }

    /// <inheritdoc/>
    public bool Equals(BigDecimal other) =>
        Coefficient.Equals(other.Coefficient) && Exponent == other.Exponent;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BigDecimal other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Coefficient, Exponent);

    /// <inheritdoc/>
    public override string ToString() => Coefficient.IsZero
        ? "0"
        : string.Create(
            CultureInfo.InvariantCulture,
            $"{Coefficient}e{Exponent}");

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (!string.IsNullOrEmpty(format) && !string.Equals(format, "G", StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"The '{format}' format is not supported.");
        }

        return ToString();
    }

    /// <inheritdoc/>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (!format.IsEmpty && !format.Equals("G", StringComparison.OrdinalIgnoreCase))
        {
            charsWritten = 0;
            return false;
        }

        string text = ToString();
        if (!text.AsSpan().TryCopyTo(destination))
        {
            charsWritten = 0;
            return false;
        }

        charsWritten = text.Length;
        return true;
    }

    public static bool operator ==(BigDecimal left, BigDecimal right) => left.Equals(right);

    public static bool operator !=(BigDecimal left, BigDecimal right) => !left.Equals(right);

    public static bool operator <(BigDecimal left, BigDecimal right) => left.CompareTo(right) < 0;

    public static bool operator <=(BigDecimal left, BigDecimal right) => left.CompareTo(right) <= 0;

    public static bool operator >(BigDecimal left, BigDecimal right) => left.CompareTo(right) > 0;

    public static bool operator >=(BigDecimal left, BigDecimal right) => left.CompareTo(right) >= 0;

    private static bool TryParseCore(
        ReadOnlySpan<char> text,
        BigDecimalProfile profile,
        out BigDecimal result)
    {
        result = default;
        if (text.IsEmpty || text.Length > profile.MaxInputLength)
        {
            return false;
        }

        int index = 0;
        bool negative = false;
        if (text[index] is '+' or '-')
        {
            negative = text[index] == '-';
            index++;
            if (index == text.Length)
            {
                return false;
            }
        }

        int coefficientStart = index;
        int integerDigits = ConsumeDigits(text, ref index);
        if (integerDigits == 0)
        {
            return false;
        }

        int fractionalDigits = 0;
        int decimalPoint = -1;
        if (index < text.Length && text[index] == '.')
        {
            decimalPoint = index++;
            fractionalDigits = ConsumeDigits(text, ref index);
            if (fractionalDigits == 0)
            {
                return false;
            }
        }

        int exponent = 0;
        if (index < text.Length && text[index] is 'e' or 'E')
        {
            index++;
            if (!TryParseExponent(text[index..], out exponent))
            {
                return false;
            }

            index = text.Length;
        }

        if (index != text.Length)
        {
            return false;
        }

        int totalDigits = checked(integerDigits + fractionalDigits);
        int firstNonZero = coefficientStart;
        int coefficientEnd = coefficientStart + totalDigits + (decimalPoint >= 0 ? 1 : 0);
        while (firstNonZero < coefficientEnd &&
               (text[firstNonZero] == '0' || text[firstNonZero] == '.'))
        {
            firstNonZero++;
        }

        int lastNonZero = coefficientEnd - 1;
        while (lastNonZero >= firstNonZero &&
               (text[lastNonZero] == '0' || text[lastNonZero] == '.'))
        {
            lastNonZero--;
        }

        int significantDigits = 0;
        for (int i = firstNonZero; i <= lastNonZero; i++)
        {
            significantDigits += text[i] == '.' ? 0 : 1;
        }

        if (significantDigits > profile.MaxCoefficientDigits)
        {
            return false;
        }

        int coefficientBufferLength = totalDigits + (negative ? 1 : 0);
        if ((long)coefficientBufferLength * sizeof(char) > profile.MaxTemporaryAllocationBytes)
        {
            return false;
        }

        long rawExponent = (long)exponent - fractionalDigits;
        if (rawExponent is < int.MinValue or > int.MaxValue)
        {
            return false;
        }

        int trailingZeroDigits = 0;
        for (int i = lastNonZero + 1; i < coefficientEnd; i++)
        {
            trailingZeroDigits += text[i] == '0' ? 1 : 0;
        }

        long normalizedExponentEstimate = significantDigits == 0
            ? 0
            : rawExponent + trailingZeroDigits;
        if (normalizedExponentEstimate < profile.MinExponent ||
            normalizedExponentEstimate > profile.MaxExponent)
        {
            return false;
        }

        Span<char> coefficientText = coefficientBufferLength <= 512
            ? stackalloc char[coefficientBufferLength]
            : new char[coefficientBufferLength];
        int destination = 0;
        if (negative)
        {
            coefficientText[destination++] = '-';
        }

        for (int i = coefficientStart; i < coefficientEnd; i++)
        {
            if (text[i] != '.')
            {
                coefficientText[destination++] = text[i];
            }
        }

        if (!BigInteger.TryParse(
                coefficientText[..destination],
                NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out BigInteger coefficient))
        {
            return false;
        }

        int normalizedExponent = (int)rawExponent;
        try
        {
            Normalize(ref coefficient, ref normalizedExponent);
            ValidateValue(coefficient, normalizedExponent, profile);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }
        catch (OverflowException)
        {
            return false;
        }

        result = new BigDecimal(coefficient, normalizedExponent, skipValidation: true);
        return true;
    }

    private BigDecimal(BigInteger coefficient, int exponent, bool skipValidation)
    {
        Coefficient = coefficient;
        Exponent = exponent;
    }

    private static int ConsumeDigits(ReadOnlySpan<char> text, ref int index)
    {
        int start = index;
        while (index < text.Length && text[index] is >= '0' and <= '9')
        {
            index++;
        }

        return index - start;
    }

    private static bool TryParseExponent(ReadOnlySpan<char> text, out int exponent)
    {
        exponent = 0;
        if (text.IsEmpty)
        {
            return false;
        }

        int index = 0;
        bool negative = false;
        if (text[index] is '+' or '-')
        {
            negative = text[index] == '-';
            index++;
            if (index == text.Length)
            {
                return false;
            }
        }

        long magnitude = 0;
        long limit = negative ? (long)int.MaxValue + 1 : int.MaxValue;
        for (; index < text.Length; index++)
        {
            char character = text[index];
            if (character is < '0' or > '9')
            {
                return false;
            }

            int digit = character - '0';
            if (magnitude > (limit - digit) / 10)
            {
                return false;
            }

            magnitude = (magnitude * 10) + digit;
        }

        exponent = negative ? checked((int)-magnitude) : (int)magnitude;
        return true;
    }

    private static void Normalize(ref BigInteger coefficient, ref int exponent)
    {
        if (coefficient.IsZero)
        {
            exponent = 0;
            return;
        }

        while (true)
        {
            BigInteger quotient = BigInteger.DivRem(coefficient, 10, out BigInteger remainder);
            if (!remainder.IsZero)
            {
                return;
            }

            exponent = checked(exponent + 1);
            coefficient = quotient;
        }
    }

    private static void ValidateValue(
        BigInteger coefficient,
        int exponent,
        BigDecimalProfile profile)
    {
        if (exponent < profile.MinExponent || exponent > profile.MaxExponent)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exponent),
                exponent,
                $"The normalized exponent must be within profile {profile.Name} " +
                $"[{profile.MinExponent}, {profile.MaxExponent}].");
        }

        int digits = coefficient.IsZero
            ? 1
            : BigInteger.Abs(coefficient).ToString(CultureInfo.InvariantCulture).Length;
        if (digits > profile.MaxCoefficientDigits)
        {
            throw new ArgumentOutOfRangeException(
                nameof(coefficient),
                $"The normalized coefficient exceeds profile {profile.Name}'s " +
                $"{profile.MaxCoefficientDigits}-digit limit.");
        }
    }
}
