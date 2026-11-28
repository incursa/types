---
workbench:
  type: spec
  workItems:
    - TASK-0005
    - TASK-0006
    - TASK-0007
  codeRefs: []
  path: /docs/30-contracts/bigdecimal-verification-plan.md
  pathHistory:
    - "C:/docs/30-contracts/bigdecimal-verification-plan.md"
---

# BigDecimal golden vectors and benchmark plan

## Summary

This plan defines independent correctness fixtures, hostile-input cases, and
performance evidence for the BigDecimal model selected by ADR-0001. TY-05
defines the evidence contract; TY-06 and TY-07 create the executable fixtures
and benchmarks.

## Goals

- Prove canonical value, equality, hash, comparison, parsing, conversion,
  arithmetic, rescaling, and rounding semantics independently of the code under
  test.
- Cover every SQL Server `DECIMAL(p,s)` boundary through 38 digits and Arrow
  Decimal128/256 boundaries through 38/76 digits.
- Demonstrate bounded failure before hostile exponent/coefficient allocation.
- Measure parse, format, compare, hash, arithmetic, and typed-column access
  separately before considering fixed-width optimization.
- Keep fixtures deterministic, reviewable, versioned, and portable.

## Non-goals

- Selecting a third-party runtime dependency.
- Treating `SqlDecimal`, Arrow, or a floating-point library as the BigDecimal
  public model.
- Benchmarking transcendental functions or unbounded arbitrary precision.
- Publishing packages or setting release performance SLAs in TY-05.

## Requirements

### Oracle independence

Fixtures are generated outside Incursa.Types and checked into the test project
as JSON Lines. Each record contains `id`, `operation`, canonical operands,
explicit context/profile, expected canonical result or exact error category,
oracle name/version, generator seed, and source-data hash.

Use these independent sources:

- Python standard-library `decimal` with a local explicit context for rounded
  results; Python integers and `fractions.Fraction` for exact reduction and
  canonical coefficient/exponent derivation.
- .NET `SqlDecimal` v8.0.34 as a second oracle only inside its 38-digit,
  nonnegative-scale SQL domain.
- Apache Arrow v23 Decimal128/256 encode/decode fixtures for scaled integer,
  precision, scale, endianness, sign extension, minimum, and maximum values.
- Hand-audited vectors for every rounding tie and failure boundary.

The generator implementation must not reference Incursa.Types. Random vectors
use a recorded seed and are materialized so ordinary test runs need no Python,
Java, network, or database.

### Fixture groups

1. **Construction and canonical text**: zero spellings, signs, decimal points,
   exponent syntax, leading/trailing zeroes, negative scales, 38/76/4,096-digit
   boundaries, malformed Unicode/culture input, and round trips.
2. **Identity**: equal spellings and scales, normalized zero, sign, stable
   hashes for equal values, and unequal adjacent magnitudes.
3. **Comparison**: signs, equal adjusted magnitudes, far exponent gaps, values
   around powers of ten, and comparison without native-number conversion.
4. **Exact conversion**: all integer widths, `BigInteger`, `decimal`, and
   `SqlDecimal`; reject fractional or out-of-range values without truncation.
5. **Exact arithmetic**: aligned/unaligned add/subtract, carry/borrow,
   multiplication, terminating division, nonterminating division, remainder,
   zero divisor, normalization, context overflow, and checked exponent math.
6. **Rounded arithmetic**: all six modes for positive/negative below-half,
   exact-half, and above-half inputs; zero; carry into a new digit; positive and
   negative target scales; no double rounding.
7. **Schema profiles**: every SQL precision boundary, representative scales
   0/p/intermediate, Arrow 128/256 min/max, schema mismatch, and exact scaled
   integer round trips.
8. **Resource safety**: preflight rejection before large powers or buffers.

Minimum hostile cases include 8,193-character text, 4,097 coefficient digits,
exponents `±4097`, exponent text outside `Int32`, normalization overflow,
`1e4096 + 1e-4096`, multiplication whose estimated result exceeds 8,192
digits, division requiring an out-of-profile terminating exponent, and repeated
`9` input near every limit. Tests should assert the public failure category and
bounded allocation, not an incidental exception message.

### Coverage mapping

| Requirement group | Required evidence |
| --- | --- |
| NUM-0001..0015 | value/canonical/parse/equality/hash/compare/bounds fixtures |
| NUM-0019, 0022..0024 | terminating/nonterminating division, quantization, all rounding ties |
| NUM-0025..0026 | exact versus explicitly lossy conversion suites |
| NUM-0027 | concurrency test proving no mutable ambient context |
| NUM-0028 | differential tests for every future fixed-width path |
| NUM-0029 | parser/converter rejection of NaN and infinities |
| NUM-0030 | interface-contract suite for each adopted generic-math interface |
| NUM-0031 | Python/Fraction + SqlDecimal evidence and recorded provenance |
| NUM-0032 | benchmark matrix below |
| NUM-0033 | shared fixture corpus consumed by Types and Tabular adapters |

### Benchmark matrix

Use a dedicated BenchmarkDotNet Release project that is not packed with the
runtime library. Record SDK, OS, architecture, CPU, GC mode, commit, and package
versions. Report throughput, latency distribution, Gen0/1/2 collections, and
allocated bytes.

| Area | Cases |
| --- | --- |
| Parse | canonical and decimal text at 9, 28, 38, 76, 512, and 4,096 digits; invalid/oversized preflight |
| Format | canonical zero, signed values, positive/negative exponent, 38/76/4,096 digits |
| Compare/hash | equal normalized values, adjacent values, aligned and 4,096-exponent-gap values |
| Add/subtract | aligned, small gap, maximum allowed gap, cancellation, carry/borrow |
| Multiply | 9×9, 38×38, 76×76, 512×512, and limit rejection |
| Divide/rescale | terminating and nonterminating, each rounding mode, ties, hostile exponent rejection |
| Conversion | `BigInteger`, `decimal`, `SqlDecimal`, Arrow 128/256 scaled integers |
| Typed columns | sequential write/read/sum/compare over 1K, 100K, and 1M Decimal38/76 values |

Baselines are `decimal` where representable, `SqlDecimal` for the SQL profile,
the straightforward owned `BigInteger` pair, and reference-library versions
only in the benchmark project. Oracle/reference packages never become runtime
dependencies.

A fixed-width optimization is considered only when differential tests are
identical and it improves at least two representative hot-path benchmarks by
20% throughput or 25% allocation with no regression over 5% elsewhere. The
evidence must include typed-column access; a microbenchmark-only win is not
sufficient.

### Delivery gates

- Fixtures have stable provenance and hashes and run on Windows and Linux.
- Release solution tests pass with no skipped BigDecimal contract suite.
- Allocation-limit tests demonstrate rejection before large allocation.
- Benchmark results are attached to the TY-07 delivery record before selecting
  any optimized storage path.
- Types and Tabular consume the same fixture corpus for a declared profile.
