#!/usr/bin/env python3
"""Verify BigDecimal core JSONL vectors with Python's independent Decimal model."""

from __future__ import annotations

import decimal
import json
import pathlib


FIXTURE = (
    pathlib.Path(__file__).parents[1]
    / "Incursa.Types.Tests"
    / "Fixtures"
    / "bigdecimal-core-vectors.jsonl"
)


def canonical_parts(text: str) -> tuple[str, int, str]:
    value = decimal.Decimal(text)
    if not value.is_finite():
        raise ValueError(f"non-finite vector: {text}")
    if value.is_zero():
        return "0", 0, "0"

    sign, digits, exponent = value.as_tuple()
    coefficient = int("".join(str(digit) for digit in digits))
    while coefficient % 10 == 0:
        coefficient //= 10
        exponent += 1
    if sign:
        coefficient = -coefficient
    coefficient_text = str(coefficient)
    return coefficient_text, exponent, f"{coefficient_text}e{exponent}"


def main() -> None:
    count = 0
    with FIXTURE.open(encoding="utf-8") as stream:
        for line_number, line in enumerate(stream, start=1):
            vector = json.loads(line)
            coefficient, exponent, canonical = canonical_parts(vector["input"])
            expected = (
                vector["coefficient"],
                vector["exponent"],
                vector["canonical"],
            )
            actual = coefficient, exponent, canonical
            if actual != expected:
                raise AssertionError(
                    f"line {line_number}: independent result {actual!r} != {expected!r}"
                )
            count += 1
    print(f"Verified {count} BigDecimal core vectors with Python decimal.")


if __name__ == "__main__":
    main()
