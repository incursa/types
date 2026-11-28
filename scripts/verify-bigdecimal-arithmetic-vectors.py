#!/usr/bin/env python3
"""Verify BigDecimal arithmetic JSONL vectors with Python Fraction."""

from __future__ import annotations

import decimal
import fractions
import json
import pathlib


FIXTURE = (
    pathlib.Path(__file__).parents[1]
    / "Incursa.Types.Tests"
    / "Fixtures"
    / "bigdecimal-arithmetic-vectors.jsonl"
)


def exact_fraction(text: str) -> fractions.Fraction:
    value = decimal.Decimal(text)
    numerator, denominator = value.as_integer_ratio()
    return fractions.Fraction(numerator, denominator)


def canonical(value: fractions.Fraction) -> str:
    if value == 0:
        return "0"

    denominator = value.denominator
    twos = 0
    while denominator % 2 == 0:
        denominator //= 2
        twos += 1
    fives = 0
    while denominator % 5 == 0:
        denominator //= 5
        fives += 1
    if denominator != 1:
        raise ValueError(f"nonterminating fraction: {value}")

    places = max(twos, fives)
    coefficient = value.numerator
    coefficient *= 2 ** (places - twos)
    coefficient *= 5 ** (places - fives)
    exponent = -places
    while coefficient % 10 == 0:
        coefficient //= 10
        exponent += 1
    return f"{coefficient}e{exponent}"


def truncated_remainder(left: fractions.Fraction, right: fractions.Fraction) -> fractions.Fraction:
    ratio = left / right
    quotient = abs(ratio.numerator) // ratio.denominator
    if ratio < 0:
        quotient = -quotient
    return left - (right * quotient)


def main() -> None:
    count = 0
    with FIXTURE.open(encoding="utf-8") as stream:
        for line_number, line in enumerate(stream, start=1):
            vector = json.loads(line)
            left = exact_fraction(vector["left"])
            right = exact_fraction(vector["right"])
            actual = {
                "add": canonical(left + right),
                "subtract": canonical(left - right),
                "multiply": canonical(left * right),
                "divide": canonical(left / right),
                "remainder": canonical(truncated_remainder(left, right)),
            }
            expected = {name: vector[name] for name in actual}
            if actual != expected:
                raise AssertionError(
                    f"line {line_number}: independent result {actual!r} != {expected!r}"
                )
            count += 1
    print(f"Verified {count} BigDecimal arithmetic vectors with Python Fraction.")


if __name__ == "__main__":
    main()
