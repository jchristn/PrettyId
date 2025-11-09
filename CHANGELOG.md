# Change Log

## Current Version

v2.0.x

**Breaking Changes:**
- Instance-based architecture (removed all static methods)
- `ValidCharacters` now `HashSet<char>` instead of `char[]`
- `GenerateBase64()` renamed to `GenerateAndEncodeBase64()`
- `MaxIterations` max increased from 64 to 256

**New Features:**
- `GenerateUrlSafe()` for RFC 3986 compliant IDs
- `GenerateKSortable()` for timestamp-prefixed chronological sorting
- `UseCryptographicRng` property for cryptographic randomness
- `UrlSafe` character set added to `DefaultCharacterSets`
- Performance warnings for restrictive character sets (<16 chars)

**Improvements:**
- StringBuilder for string building (O(n) vs O(n²))
- HashSet for character lookups (O(1) vs O(n))
- Enhanced error messages with contextual details
- Automated test suite with 43 tests

## Previous Versions

v1.0.x

- Initial release
- Added support for generating valid base64 values
