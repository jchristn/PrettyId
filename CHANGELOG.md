# Change Log

## Current Version

v2.0.1

**Bug Fixes:**
- Fixed thread safety issue in `GenerateUrlSafe()` and `GenerateAndEncodeBase64()` — these methods no longer temporarily mutate instance state
- Replaced console warning for restrictive character sets with `ArgumentException` (libraries should not write to stdout)

**Improvements:**
- Replaced rejection-sampling loop with direct modular indexing over random bytes — O(n) guaranteed, no iteration limit needed
- `RandomNumberGenerator` instance is now reused (static field) instead of being created and disposed on every call
- `DefaultCharacterSets` arrays changed from `static` to `static readonly` to prevent accidental mutation
- `MaxIterations` property retained for backward API compatibility but is no longer used internally
- Character sets with fewer than 16 unique characters now throw `ArgumentException` at construction time

## Previous Versions

v2.0.0

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

**Improvements:**
- StringBuilder for string building (O(n) vs O(n²))
- HashSet for character lookups (O(1) vs O(n))
- Enhanced error messages with contextual details
- Automated test suite with 43 tests

v1.0.x

- Initial release
- Added support for generating valid base64 values
