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
