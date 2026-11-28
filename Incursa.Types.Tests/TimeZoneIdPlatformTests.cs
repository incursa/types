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

namespace Incursa.Types.Tests;

public sealed class TimeZoneIdPlatformTests
{
    [Theory]
    [InlineData("Pacific Standard Time", "America/Los_Angeles")]
    [InlineData("Eastern Standard Time", "America/New_York")]
    [InlineData("UTC", "Etc/UTC")]
    public void WindowsIdentifiersCanonicalizeToIana(string input, string expected)
    {
        TimeZoneId timeZone = TimeZoneId.Parse(input);

        timeZone.CanonicalId.ShouldBe(expected);
    }

    [Theory]
    [InlineData("America/Los_Angeles")]
    [InlineData("America/New_York")]
    [InlineData("Etc/UTC")]
    [InlineData("US/Pacific")]
    public void IanaIdentifiersAndAliasesPreserveAcceptedInput(string input)
    {
        TimeZoneId timeZone = TimeZoneId.Parse(input);

        timeZone.CanonicalId.ShouldBe(input);
        timeZone.TimeZoneInfo.HasIanaId.ShouldBeTrue();
    }

    [Fact]
    public void InvalidIdentifierFailsWithoutSubstitutingHostZone()
    {
        const string InvalidIdentifier = "Not/A_Real_Time_Zone";

        Should.Throw<ArgumentException>(() => TimeZoneId.Parse(InvalidIdentifier));
        TimeZoneId.TryParse(InvalidIdentifier, out TimeZoneId result).ShouldBeFalse();
        result.ShouldBe(default);
    }

    [Fact]
    public void PlatformConversionDataSupportsWindowsAndIanaMappings()
    {
        TimeZoneInfo.TryConvertWindowsIdToIanaId("Pacific Standard Time", out string? ianaId).ShouldBeTrue();
        ianaId.ShouldBe("America/Los_Angeles");
        TimeZoneInfo.TryConvertIanaIdToWindowsId("America/Los_Angeles", out string? windowsId).ShouldBeTrue();
        windowsId.ShouldBe("Pacific Standard Time");
    }

    [Fact]
    public void InvariantGlobalizationReportsUnavailablePlatformData()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"),
                "1",
                StringComparison.Ordinal))
        {
            return;
        }

        PlatformNotSupportedException exception = Should.Throw<PlatformNotSupportedException>(
            () => TimeZoneId.Parse("Pacific Standard Time"));

        exception.Message.ShouldContain("ICU/CLDR");
    }
}
