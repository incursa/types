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

public readonly partial record struct EncryptedString
{
    static partial void ProcessValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Encrypted payload cannot be empty.", nameof(value));
        }

        // Encrypted payloads are expected as Base64-encoded ciphertext blobs.
        // This keeps the type contract explicit and rejects plain-text values.
        if (value.Length < 16 || (value.Length % 4) != 0)
        {
            throw new FormatException("Encrypted payload must be valid Base64 ciphertext.");
        }

        try
        {
            byte[] decoded = Convert.FromBase64String(value);
            if (decoded.Length < 12)
            {
                throw new FormatException("Encrypted payload is too short to represent ciphertext.");
            }
        }
        catch (FormatException ex)
        {
            throw new FormatException("Encrypted payload must be valid Base64 ciphertext.", ex);
        }
    }
}
