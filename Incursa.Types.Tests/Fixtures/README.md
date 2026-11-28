# BigDecimal core vectors

`bigdecimal-core-vectors.jsonl` covers zero normalization, cosmetic scale,
scientific notation, negative scale, the 38-digit SQL envelope, and native
decimal boundaries. Expected coefficient, exponent, and canonical text are
verified independently with Python's standard-library `decimal.Decimal` by:

```text
python scripts/verify-bigdecimal-core-vectors.py
```

The fixture contains no output copied from the Incursa implementation. Runtime
tests consume the verified JSONL and separately compare mandatory precision
and scale boundaries with `System.Data.SqlTypes.SqlDecimal`.

`bigdecimal-arithmetic-vectors.jsonl` was calculated independently as integer
coefficient/exponent fractions and covers exact add, subtract, multiply,
terminating divide, and truncated remainder. The arithmetic verifier extends
`scripts/verify-bigdecimal-arithmetic-vectors.py` uses
`fractions.Fraction`; runtime tests consume those
results and separately compare compatible operations with `SqlDecimal`.

`elapsed-duration-vectors.jsonl` records exact signed Int64 100-nanosecond
boundaries and canonical seconds text. `local-date-time-vectors.jsonl` records
the Gregorian year bounds, leap-day rules, and exact seven-digit local-field
encoding. They are portable value/lexical fixtures with no host timezone
dependency; zone behavior is exercised separately against explicit IANA data.
