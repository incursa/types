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

namespace Incursa;

/// <summary>
/// Defines immutable resource and value limits for <see cref="BigDecimal"/>.
/// </summary>
public sealed record BigDecimalProfile
{
    /// <summary>
    /// Gets the mandatory SQL Server DECIMAL and Arrow Decimal128 profile.
    /// </summary>
    public static BigDecimalProfile Decimal38 { get; } = new(
        nameof(Decimal38),
        38,
        -38,
        38,
        256,
        128,
        4 * 1024);

    /// <summary>
    /// Gets the optional Arrow Decimal256 profile.
    /// </summary>
    public static BigDecimalProfile Decimal76 { get; } = new(
        nameof(Decimal76),
        76,
        -76,
        76,
        512,
        256,
        8 * 1024);

    /// <summary>
    /// Gets the bounded general-purpose profile used by default.
    /// </summary>
    public static BigDecimalProfile VariableSafe { get; } = new(
        nameof(VariableSafe),
        4096,
        -4096,
        4096,
        8192,
        8192,
        64 * 1024);

    /// <summary>
    /// Initializes a new instance of the <see cref="BigDecimalProfile"/> class.
    /// </summary>
    public BigDecimalProfile(
        string name,
        int maxCoefficientDigits,
        int minExponent,
        int maxExponent,
        int maxInputLength,
        int maxWorkingDigits,
        int maxTemporaryAllocationBytes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCoefficientDigits);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minExponent, maxExponent);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxInputLength);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxInputLength, maxCoefficientDigits);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxWorkingDigits, maxCoefficientDigits);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxTemporaryAllocationBytes);

        Name = name;
        MaxCoefficientDigits = maxCoefficientDigits;
        MinExponent = minExponent;
        MaxExponent = maxExponent;
        MaxInputLength = maxInputLength;
        MaxWorkingDigits = maxWorkingDigits;
        MaxTemporaryAllocationBytes = maxTemporaryAllocationBytes;
    }

    /// <summary>Gets the profile name.</summary>
    public string Name { get; }

    /// <summary>Gets the maximum normalized coefficient digit count.</summary>
    public int MaxCoefficientDigits { get; }

    /// <summary>Gets the minimum normalized exponent.</summary>
    public int MinExponent { get; }

    /// <summary>Gets the maximum normalized exponent.</summary>
    public int MaxExponent { get; }

    /// <summary>Gets the maximum accepted parse input length.</summary>
    public int MaxInputLength { get; }

    /// <summary>Gets the maximum estimated working coefficient digit count.</summary>
    public int MaxWorkingDigits { get; }

    /// <summary>Gets the maximum owned temporary allocation budget in bytes.</summary>
    public int MaxTemporaryAllocationBytes { get; }
}
