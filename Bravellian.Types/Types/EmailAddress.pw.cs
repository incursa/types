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

namespace Bravellian;

using System;

public readonly partial record struct EmailAddress
{
    private static partial void ProcessValue(string value, out string normalizedValue, out System.Net.Mail.MailAddress address)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var parsed = MimeKit.MailboxAddress.Parse(value);
        if (string.IsNullOrWhiteSpace(parsed.Address))
        {
            throw new ArgumentException("Email address must contain an '@' symbol", nameof(value));
        }

        string[] segments = parsed.Address.Split('@');
        if (segments.Length != 2)
        {
            throw new ArgumentException("Email address must contain an '@' symbol", nameof(value));
        }

        var idn = new System.Globalization.IdnMapping();
        string normalizedDomain = idn.GetAscii(segments[1].Trim()).ToLowerInvariant();
        string localPart = segments[0].Trim();

        normalizedValue = $"{localPart.ToLowerInvariant()}@{normalizedDomain}";
        address = new(normalizedValue, parsed.Name);
    }
}
