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
/// Supplies immutable, explicit resource limits for BigDecimal operations.
/// </summary>
public sealed record BigDecimalContext
{
    /// <summary>Gets the stable default VariableSafe operation context.</summary>
    public static BigDecimalContext Default { get; } = new(BigDecimalProfile.VariableSafe);

    /// <summary>Gets the mandatory Decimal38 operation context.</summary>
    public static BigDecimalContext Decimal38 { get; } = new(BigDecimalProfile.Decimal38);

    /// <summary>Gets the optional Decimal76 operation context.</summary>
    public static BigDecimalContext Decimal76 { get; } = new(BigDecimalProfile.Decimal76);

    /// <summary>Initializes a new context using <paramref name="profile"/>.</summary>
    public BigDecimalContext(BigDecimalProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Profile = profile;
    }

    /// <summary>Gets the value and resource profile for this context.</summary>
    public BigDecimalProfile Profile { get; }
}
