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
