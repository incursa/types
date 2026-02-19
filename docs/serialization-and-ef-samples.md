# Serialization and EF Core samples

The types in this library ship with JSON converters and type converters for predictable persistence. The snippets below illustrate usage patterns for both serialization and Entity Framework Core.

## System.Text.Json
```csharp
var fastId = FastId.FromGuidWithinTimestampRange(Guid.Parse("c5c6cb5f-8b51-4d26-9301-9919549b1af0"), DateTimeOffset.UtcNow);
var money = new Money(12.34m);
var state = UsaState.Texas;

var json = JsonSerializer.Serialize(new { fastId, money, state });
var roundTrip = JsonSerializer.Deserialize<JsonElement>(json);
```

All value objects register `JsonConverter` attributes so no custom `JsonSerializerOptions` are required.

### Important behavior guarantees
* Numeric-backed value objects (`Money`, `Percentage`) serialize and parse using invariant culture.
* Converter inputs are strict. Invalid strings throw during JSON deserialization or `TypeConverter` conversion.
* Default instances are handled safely for core value structs and do not silently coerce bad input.

## TypeConverter behavior
```csharp
var converter = TypeDescriptor.GetConverter(typeof(EmailAddress));

// Valid
var value = (EmailAddress)converter.ConvertFrom("user@example.com")!;

// Invalid values now fail fast
Assert.Throws<FormatException>(() => converter.ConvertFrom("not-an-email"));
```

## EF Core value converters
```csharp
// FastId as base32 string
var fastIdConverter = new ValueConverter<FastId, string>(
    v => v.Encoded,
    v => FastId.Parse(v));

// Money as decimal
var moneyConverter = new ValueConverter<Money, decimal>(
    v => v.Value,
    v => new Money(v));

// UsaState as postal abbreviation
var usaStateConverter = new ValueConverter<UsaState, string>(
    v => v.Value,
    v => UsaState.From(v));

// Percentage as decimal fractional value
var percentageConverter = new ValueConverter<Percentage, decimal>(
    v => v.Value,
    v => new Percentage(v));
```

These converters can be applied in `OnModelCreating`:
```csharp
builder.Entity<Order>().Property(o => o.OrderId).HasConversion(fastIdConverter);
builder.Entity<Order>().Property(o => o.Total).HasConversion(moneyConverter);
builder.Entity<Order>().Property(o => o.State).HasConversion(usaStateConverter);
builder.Entity<Order>().Property(o => o.TaxRate).HasConversion(percentageConverter);
```

When adding new value objects, follow the same pattern: create predictable JSON and EF Core conversions and include unit tests that round-trip between the model type and provider value.
