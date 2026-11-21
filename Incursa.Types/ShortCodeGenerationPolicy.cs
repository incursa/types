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

internal static class ShortCodeGenerationPolicy
{
    // This is intentionally a compact, reviewed generation policy rather than a
    // general-purpose text filter. Terms are limited to fragments that the
    // ShortCode alphabet can produce. Inclusion means unsuitable for an
    // uncontextualized generated code, not universally offensive in all uses.
    // See the ShortCode behavioral specification for scope and provenance.
    private static readonly string[] UnwantedFragments =
    [
        // English profanity, hostile slang, and slurs.
        "ASS",
        "CUM",
        "FAG",
        "FCK",
        "FUK",
        "GYP",
        "JAP",
        "KKK",
        "SEX",
        "WTF",
        "ANUS",
        "ARSE",
        "CRAP",
        "CUCK",
        "CUNT",
        "DAMN",
        "DYKE",
        "FART",
        "FUCK",
        "HEEB",
        "KYKE",
        "NUDE",
        "PERV",
        "PUKE",
        "PUSS",
        "RAPE",
        "SCAT",
        "SHAG",
        "SMUT",
        "STFU",
        "SUCK",
        "TARD",
        "TURD",
        "TWAT",
        "WANK",
        "BASTARD",
        "BEANER",
        "KRAUT",
        "NASTY",
        "NSFW",
        "QUEER",
        "RAGHEAD",
        "SPERM",
        "WETBACK",

        // Spanish.
        "CACA",
        "CUCA",
        "PENE",
        "PERRA",
        "PUTA",
        "VERGA",

        // French.
        "MERDE",
        "PUTE",
        "TEUB",

        // German and Dutch.
        "ARSCH",
        "HURE",
        "KUT",
        "NEUKEN",
        "SCHWANZ",

        // Italian.
        "CACCA",
        "FESSA",
        "FREGNA",
        "MERDA",
        "PUTTANA",
        "TETTE",

        // Portuguese.
        "BABACA",
        "BUCETA",
        "BUNDA",
        "PUNHETA",
        "RABUDA",
        "XERECA",

        // Arabic represented in Latin script.
        "AYAAR",
        "CAHBA",
        "GAWAD",
        "KHANZEER",
        "MANYAK",
        "METNAK",
        "SHARMUTA",
        "ZEBY",
    ];

    internal static bool IsAcceptable(ReadOnlySpan<char> candidate)
    {
        Span<char> normalized = stackalloc char[candidate.Length];
        for (var i = 0; i < candidate.Length; i++)
        {
            normalized[i] = NormalizeDigitSubstitution(candidate[i]);
        }

        foreach (var fragment in UnwantedFragments)
        {
            if (candidate.Contains(fragment, StringComparison.Ordinal)
                || normalized.Contains(fragment, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static char NormalizeDigitSubstitution(char value) => value switch
    {
        '2' => 'Z',
        '3' => 'E',
        '4' => 'A',
        '5' => 'S',
        '7' => 'T',
        '8' => 'B',
        '9' => 'G',
        _ => value,
    };
}
