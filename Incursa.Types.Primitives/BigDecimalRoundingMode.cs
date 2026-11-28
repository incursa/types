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

/// <summary>Defines explicit rounding policies for BigDecimal operations.</summary>
public enum BigDecimalRoundingMode
{
    /// <summary>Rounds toward zero.</summary>
    TowardZero,

    /// <summary>Rounds away from zero.</summary>
    AwayFromZero,

    /// <summary>Rounds toward positive infinity.</summary>
    TowardPositiveInfinity,

    /// <summary>Rounds toward negative infinity.</summary>
    TowardNegativeInfinity,

    /// <summary>Rounds to the nearest value, with ties going to an even coefficient.</summary>
    ToNearestTiesToEven,

    /// <summary>Rounds to the nearest value, with ties going away from zero.</summary>
    ToNearestTiesAwayFromZero,
}
