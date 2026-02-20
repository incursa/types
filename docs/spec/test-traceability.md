---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/test-traceability.md"
  path: /docs/spec/test-traceability.md
---

# Spec to Test Traceability

This file maps behavioral specification clauses to concrete tests.

## Format
- `SPEC-ID`: stable scenario identifier from the type spec
- `Spec Location`: file and section
- `Test`: fully qualified test method(s)
- `Status`: `Planned|Implemented`

## Core Types (Approved Specs)

| SPEC-ID | Spec Location | Test | Status |
| --- | --- | --- | --- |
| MONEY-NORM-001 | `docs/spec/types/Money.md` / Normalization Rules | `Incursa.Types.Tests.MoneyTests.ToDecimal_NormalizesUsingBankersRounding` | Implemented |
| MONEY-PARSE-001 | `docs/spec/types/Money.md` / Parse/TryParse | `Incursa.Types.Tests.MoneyTests.Parse_AcceptsInvariantNumericForms` | Implemented |
| MONEY-PARSE-002 | `docs/spec/types/Money.md` / Parse/TryParse | `Incursa.Types.Tests.MoneyTests.TryParse_InvalidValue_ReturnsFalse` | Implemented |
| MONEY-CONVERT-001 | `docs/spec/types/Money.md` / Converters/Serialization | `Incursa.Types.Tests.MoneyTests.JsonConverter_RoundTripsInvariantValue` | Implemented |
| MONEY-CONVERT-002 | `docs/spec/types/Money.md` / Converters/Serialization | `Incursa.Types.Tests.MoneyTests.TypeConverter_ConvertsToAndFromString` | Implemented |
| PCT-NORM-001 | `docs/spec/types/Percentage.md` / Normalization Rules | `Incursa.Types.Tests.PercentageTests.Value_TruncatesToFourDecimalPlaces` | Implemented |
| PCT-PARSE-001 | `docs/spec/types/Percentage.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.Percentage_Formatting_Parsing_Converters_AndComparison_AreConsistent` | Implemented |
| PCT-SCALED-001 | `docs/spec/types/Percentage.md` / ParseScaled | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.Percentage_ParseScaled_InterpretsScaledInput` | Implemented |
| PCT-PARSE-INVALID-001 | `docs/spec/types/Percentage.md` / ParseScaled | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.Percentage_TryParseScaled_InvalidInput_ReturnsFalse` | Implemented |
| PCT-FORMAT-001 | `docs/spec/types/Percentage.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.Percentage_Formatting_Parsing_Converters_AndComparison_AreConsistent`; `Incursa.Types.Tests.PercentageTests.ToString_WithZeroDecimals_UsesIntegerPercentFormat` | Implemented |
| PCT-CONVERT-001 | `docs/spec/types/Percentage.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.Percentage_Formatting_Parsing_Converters_AndComparison_AreConsistent`; `Incursa.Types.Tests.PercentageTests.TypeConverter_ConvertTo_UnsupportedDestination_Throws` | Implemented |
| DURATION-RT-001 | `docs/spec/types/Duration.md` / Formatting | `Incursa.Types.Tests.DurationAndPeriodTests.Duration_Parse_RoundTripsCommonPatterns` | Implemented |
| DURATION-PARSE-INVALID-001 | `docs/spec/types/Duration.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.Duration_Parse_Invalid_ThrowsFormatException` | Implemented |
| PERIOD-BOUNDARY-001 | `docs/spec/types/Period.md` / Interval semantics | `Incursa.Types.Tests.HardeningRegressionTests.Period_EndExclusiveBoundary_IsNotContained` | Implemented |
| PERIOD-PARSE-INVALID-001 | `docs/spec/types/Period.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.Period_Parse_Invalid_ThrowsFormatException` | Implemented |
| RECUR-DET-001 | `docs/spec/types/RecurringPeriod.md` / GetPeriod | `Incursa.Types.Tests.HardeningRegressionTests.RecurringPeriod_UsesProvidedStartTime` | Implemented |
| RECUR-PARSE-INVALID-001 | `docs/spec/types/RecurringPeriod.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.RecurringPeriod_Parse_Invalid_ReturnsNullAndFalse` | Implemented |
| FASTID-EMPTY-001 | `docs/spec/types/FastId.md` / Default semantics | `Incursa.Types.Tests.HardeningRegressionTests.FastId_Default_IsSafe` | Implemented |
| FASTID-PARSE-INVALID-001 | `docs/spec/types/FastId.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.FastId_TryParse_InvalidCharacter_ReturnsFalse` | Implemented |
| FASTID-CONVERT-001 | `docs/spec/types/FastId.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenCoreTypesTests.FastId_JsonConverter_RoundTripsEncodedValue` | Implemented |

## Supporting Types

| SPEC-ID | Spec Location | Test | Status |
| --- | --- | --- | --- |
| VIRTUALPATH-CONSTRUCT-001 | `docs/spec/types/VirtualPath.md` / Construction | `Incursa.Types.Tests.AdditionalValueObjectTests.VirtualPath_CombineAndEquality` | Implemented |
| VIRTUALPATH-CONVERT-001 | `docs/spec/types/VirtualPath.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.VirtualPathExtensions_FileOperations_AreConsistent` | Implemented |
| SHORTCODE-PARSE-001 | `docs/spec/types/ShortCode.md` / Parse/TryParse | `Incursa.Types.Tests.HardeningRegressionTests.ShortCode_TryParse_RejectsInvalidAndNormalizes` | Implemented |
| SHORTCODE-CONSTRUCT-001 | `docs/spec/types/ShortCode.md` / Construction | `Incursa.Types.Tests.HardeningRegressionTests.ShortCode_Default_IsSafe` | Implemented |
| SHORTCODE-TRYPARSE-001 | `docs/spec/types/ShortCode.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.ShortCode_Parse_Generate_Format_And_Converters_AreConsistent` | Implemented |
| SHORTCODE-FORMAT-001 | `docs/spec/types/ShortCode.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.ShortCode_Parse_Generate_Format_And_Converters_AreConsistent` | Implemented |
| SHORTCODE-CONVERT-001 | `docs/spec/types/ShortCode.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.ShortCode_Parse_Generate_Format_And_Converters_AreConsistent` | Implemented |
| BVFILE-CONSTRUCT-001 | `docs/spec/types/BvFile.md` / Construction | `Incursa.Types.Tests.AdditionalValueObjectTests.BvFile_FromBase64_CapturesMetadata` | Implemented |
| BVFILE-PARSE-001 | `docs/spec/types/BvFile.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.BvFile_FromBase64_DefaultOverload_UsesDefaults` | Implemented |
| BVFILE-TRYPARSE-001 | `docs/spec/types/BvFile.md` / Factory Validation | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.BvFile_PathFactories_ValidateInputs_AndResolveMimeTypes` | Implemented |
| BVFILE-FORMAT-001 | `docs/spec/types/BvFile.md` / Object Serialization | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.BvFile_JsonConstructor_RoundTrips` | Implemented |
| BVFILE-CONVERT-001 | `docs/spec/types/BvFile.md` / Base64 Factories | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.BvFile_Constructors_And_Factories_PreserveMetadataAndData` | Implemented |
| MAYBE-CONSTRUCT-001 | `docs/spec/types/Maybe.md` / Construction | `Incursa.Types.Tests.AdditionalValueObjectTests.Maybe_MatchAndSelect` | Implemented |
| MAYBE-TRYPARSE-001 | `docs/spec/types/Maybe.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.Maybe_TryGetValue_None_ReturnsFalse` | Implemented |
| EMAILADDRESS-CONSTRUCT-001 | `docs/spec/types/EmailAddress.md` / Construction | `Incursa.Types.Tests.NewValueObjectsTests.EmailAddress_Normalizes_Domain_And_Local` | Implemented |
| EMAILADDRESS-PARSE-001 | `docs/spec/types/EmailAddress.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.EmailAddress_ParseTryParse_And_Converters_UseNormalizedValue` | Implemented |
| EMAILADDRESS-TRYPARSE-001 | `docs/spec/types/EmailAddress.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.EmailAddress_ParseTryParse_And_Converters_UseNormalizedValue` | Implemented |
| EMAILADDRESS-FORMAT-001 | `docs/spec/types/EmailAddress.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.EmailAddress_ParseTryParse_And_Converters_UseNormalizedValue` | Implemented |
| EMAILADDRESS-CONVERT-001 | `docs/spec/types/EmailAddress.md` / Converters/Serialization | `Incursa.Types.Tests.HardeningRegressionTests.TypeConverters_FailFast_OnInvalidInput` | Implemented |
| PHONENUMBER-CONSTRUCT-001 | `docs/spec/types/PhoneNumber.md` / Construction | `Incursa.Types.Tests.NewValueObjectsTests.PhoneNumber_Formats_To_E164` | Implemented |
| PHONENUMBER-PARSE-001 | `docs/spec/types/PhoneNumber.md` / Parse/TryParse | `Incursa.Types.Tests.NewValueObjectsTests.PhoneNumber_Rejects_ShortCodes` | Implemented |
| PHONENUMBER-TRYPARSE-001 | `docs/spec/types/PhoneNumber.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.PhoneNumber_Parse_TryParse_Compare_And_Converters_AreConsistent` | Implemented |
| PHONENUMBER-FORMAT-001 | `docs/spec/types/PhoneNumber.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.PhoneNumber_Parse_TryParse_Compare_And_Converters_AreConsistent` | Implemented |
| PHONENUMBER-CONVERT-001 | `docs/spec/types/PhoneNumber.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.PhoneNumber_Parse_TryParse_Compare_And_Converters_AreConsistent` | Implemented |
| COUNTRYCODE-PARSE-001 | `docs/spec/types/CountryCode.md` / Parse/TryParse | `Incursa.Types.Tests.NewValueObjectsTests.CountryCode_Parses_Two_And_Three_Letter_Forms` | Implemented |
| COUNTRYCODE-CONSTRUCT-001 | `docs/spec/types/CountryCode.md` / Construction | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CountryCode_Parse_AcceptsAlpha2_Alpha3_AndCultureTags` | Implemented |
| COUNTRYCODE-TRYPARSE-001 | `docs/spec/types/CountryCode.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CountryCode_TryParse_Format_And_Converters_BehaveAsSpecified` | Implemented |
| COUNTRYCODE-FORMAT-001 | `docs/spec/types/CountryCode.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CountryCode_TryParse_Format_And_Converters_BehaveAsSpecified` | Implemented |
| COUNTRYCODE-CONVERT-001 | `docs/spec/types/CountryCode.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CountryCode_TryParse_Format_And_Converters_BehaveAsSpecified` | Implemented |
| CURRENCYCODE-PARSE-001 | `docs/spec/types/CurrencyCode.md` / Parse/TryParse | `Incursa.Types.Tests.NewValueObjectsTests.CurrencyCode_Provides_Metadata` | Implemented |
| CURRENCYCODE-CONSTRUCT-001 | `docs/spec/types/CurrencyCode.md` / Construction | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CurrencyCode_Parse_NormalizesAndPopulatesMetadata` | Implemented |
| CURRENCYCODE-TRYPARSE-001 | `docs/spec/types/CurrencyCode.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CurrencyCode_TryParse_And_Format_APIs_AreConsistent` | Implemented |
| CURRENCYCODE-FORMAT-001 | `docs/spec/types/CurrencyCode.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CurrencyCode_TryParse_And_Format_APIs_AreConsistent` | Implemented |
| CURRENCYCODE-CONVERT-001 | `docs/spec/types/CurrencyCode.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.CurrencyCode_Converters_RoundTrip_And_RejectInvalidInput` | Implemented |
| LOCALE-CONSTRUCT-001 | `docs/spec/types/Locale.md` / Construction | `Incursa.Types.Tests.NewValueObjectsTests.Locale_Normalizes_Bcp47` | Implemented |
| LOCALE-CONVERT-001 | `docs/spec/types/Locale.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.Locale_TypeConverter_RoundTripsString` | Implemented |
| TIMEZONEID-PARSE-001 | `docs/spec/types/TimeZoneId.md` / Parse/TryParse | `Incursa.Types.Tests.NewValueObjectsTests.TimeZoneId_Bridges_Windows_And_Iana` | Implemented |
| TIMEZONEID-TRYPARSE-001 | `docs/spec/types/TimeZoneId.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.TimeZoneId_TryParse_Invalid_ReturnsFalse`; `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.TimeZoneId_TryParse_DoesNotThrow_OnInvalidInput` | Implemented |
| TIMEZONEID-CONSTRUCT-001 | `docs/spec/types/TimeZoneId.md` / Construction | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.TimeZoneId_Parse_And_Converters_ResolveCanonicalIdentifier` | Implemented |
| TIMEZONEID-FORMAT-001 | `docs/spec/types/TimeZoneId.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.TimeZoneId_Parse_And_Converters_ResolveCanonicalIdentifier` | Implemented |
| TIMEZONEID-CONVERT-001 | `docs/spec/types/TimeZoneId.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.TimeZoneId_Parse_And_Converters_ResolveCanonicalIdentifier`; `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.TimeZoneId_TypeConverter_UnsupportedDestination_Throws` | Implemented |
| URL-CONSTRUCT-001 | `docs/spec/types/Url.md` / Construction | `Incursa.Types.Tests.NewValueObjectsTests.Url_Normalizes_Host_And_Default_Port` | Implemented |
| URL-PARSE-001 | `docs/spec/types/Url.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.Url_Normalization_And_Converters_AreDeterministic` | Implemented |
| URL-TRYPARSE-001 | `docs/spec/types/Url.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.Url_TryParse_RelativeUrl_IsNotAbsolute` | Implemented |
| URL-FORMAT-001 | `docs/spec/types/Url.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.Url_Normalization_And_Converters_AreDeterministic` | Implemented |
| URL-CONVERT-001 | `docs/spec/types/Url.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.Url_Normalization_And_Converters_AreDeterministic` | Implemented |
| IPADDRESS-PARSE-001 | `docs/spec/types/IpAddress.md` / Parse/TryParse | `Incursa.Types.Tests.NewValueObjectsTests.IpAddress_Parses_V4_And_V6` | Implemented |
| IPADDRESS-CONSTRUCT-001 | `docs/spec/types/IpAddress.md` / Construction | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.IpAddress_Parse_TryParse_And_Converters_HandleV4AndV6` | Implemented |
| IPADDRESS-TRYPARSE-001 | `docs/spec/types/IpAddress.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.IpAddress_Parse_TryParse_And_Converters_HandleV4AndV6` | Implemented |
| IPADDRESS-FORMAT-001 | `docs/spec/types/IpAddress.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.IpAddress_Parse_TryParse_And_Converters_HandleV4AndV6` | Implemented |
| IPADDRESS-CONVERT-001 | `docs/spec/types/IpAddress.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.IpAddress_Converters_RoundTrip_And_RejectInvalidInput` | Implemented |
| CIDRRANGE-PARSE-001 | `docs/spec/types/CidrRange.md` / Parse/TryParse | `Incursa.Types.Tests.NewValueObjectsTests.CidrRange_Checks_Containment` | Implemented |
| CIDRRANGE-CONSTRUCT-001 | `docs/spec/types/CidrRange.md` / Construction | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.CidrRange_PrefixZero_ContainsAnyAddressInFamily` | Implemented |
| USASTATE-PARSE-001 | `docs/spec/types/UsaState.md` / Parse/TryParse | `Incursa.Types.Tests.UsaStateTests.AllStates_AreExhaustiveAndCaseInsensitive` | Implemented |
| USASTATE-TRYPARSE-001 | `docs/spec/types/UsaState.md` / Parse/TryParse | `Incursa.Types.Tests.UsaStateTests.TryParse_RejectsUnknownValues` | Implemented |
| USASTATE-CONSTRUCT-001 | `docs/spec/types/UsaState.md` / Construction | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.UsaState_Converters_And_Parse_APIs_RoundTrip` | Implemented |
| USASTATE-FORMAT-001 | `docs/spec/types/UsaState.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.UsaState_Converters_And_Parse_APIs_RoundTrip` | Implemented |
| USASTATE-CONVERT-001 | `docs/spec/types/UsaState.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenLowCoverageTypesTests.UsaState_Converters_And_Parse_APIs_RoundTrip` | Implemented |
| JSONCONTEXT-CONSTRUCT-001 | `docs/spec/types/JsonContext.md` / Construction | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.JsonContext_Constructor_Null_Throws` | Implemented |
| JSONCONTEXT-PARSE-001 | `docs/spec/types/JsonContext.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.JsonContext_Parse_TryParse_And_JsonConverter_FollowObjectContract` | Implemented |
| JSONCONTEXT-TRYPARSE-001 | `docs/spec/types/JsonContext.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.JsonContext_Parse_TryParse_And_JsonConverter_FollowObjectContract` | Implemented |
| JSONCONTEXT-FORMAT-001 | `docs/spec/types/JsonContext.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.JsonContext_Default_ReadsAsEmptyObject_ButCannotMutate` | Implemented |
| JSONCONTEXT-CONVERT-001 | `docs/spec/types/JsonContext.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.JsonContext_TypeConverter_RoundTripsString` | Implemented |
| MONTHONLY-CONSTRUCT-001 | `docs/spec/types/MonthOnly.md` / Construction | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.MonthOnly_Boundaries_And_Arithmetic_AreDeterministic` | Implemented |
| MONTHONLY-PARSE-001 | `docs/spec/types/MonthOnly.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.MonthOnly_Parse_And_ToString_AreCanonical` | Implemented |
| MONTHONLY-TRYPARSE-001 | `docs/spec/types/MonthOnly.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.MonthOnly_Parse_And_Converters_UseCanonicalYearMonth`; `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.MonthOnly_TryParse_InvalidMonth_ReturnsFalseWithoutThrowing`; `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.MonthOnly_TryParse_AcceptsYearBoundaries` | Implemented |
| MONTHONLY-FORMAT-001 | `docs/spec/types/MonthOnly.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.MonthOnly_Parse_And_Converters_UseCanonicalYearMonth` | Implemented |
| MONTHONLY-CONVERT-001 | `docs/spec/types/MonthOnly.md` / Converters/Serialization | `Incursa.Types.Tests.SpecDrivenSupportingTypesTests.MonthOnly_JsonConverter_RoundTrips` | Implemented |
| ENCRYPTEDSTRING-CONSTRUCT-001 | `docs/spec/types/EncryptedString.md` / Construction | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.EncryptedString_Validation_RejectsMalformedCiphertext` | Implemented |
| ENCRYPTEDSTRING-PARSE-001 | `docs/spec/types/EncryptedString.md` / Parse/TryParse | `Incursa.Types.Tests.HardeningRegressionTests.EncryptedString_RequiresBase64Ciphertext` | Implemented |
| ENCRYPTEDSTRING-TRYPARSE-001 | `docs/spec/types/EncryptedString.md` / Parse/TryParse | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.EncryptedString_Validation_RejectsMalformedCiphertext` | Implemented |
| ENCRYPTEDSTRING-FORMAT-001 | `docs/spec/types/EncryptedString.md` / Formatting/ToString | `Incursa.Types.Tests.SpecDrivenWaveOneHardeningTests.EncryptedString_Converters_And_Generation_RoundTrip` | Implemented |
| ENCRYPTEDSTRING-CONVERT-001 | `docs/spec/types/EncryptedString.md` / Converters/Serialization | `Incursa.Types.Tests.HardeningRegressionTests.EncryptedString_RequiresBase64Ciphertext` | Implemented |
