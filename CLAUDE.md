# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

PrettyId is a .NET library for generating uniform, readable IDs similar to those used by Stripe. It generates base64-encoded GUID strings of specified length with optional prefixes.

## Build and Development Commands

### Building the Library
```bash
# Build the library (generates NuGet package automatically)
dotnet build src/PrettyId/PrettyId.csproj

# Build in Release mode
dotnet build src/PrettyId/PrettyId.csproj -c Release

# Build the entire solution
dotnet build src/PrettyId.sln
```

### Running Tests
The project uses an automated test suite with PASS/FAIL validation:
```bash
# Run the automated test suite
dotnet run --project src/Test/Test.csproj

# Run in Release mode
dotnet run --project src/Test/Test.csproj -c Release

# Run with specific framework
dotnet run --project src/Test/Test.csproj --framework net8.0
```

The test suite includes 43+ tests covering:
- Basic ID generation (default, custom lengths, character validation, uniqueness)
- Prefix generation (with various prefix types and edge cases)
- Custom character sets (hex, base64, URL-safe, numeric)
- URL-safe ID generation
- K-sortable (timestamp-based) ID generation
- Base64-encoded ID generation
- Cryptographic RNG generation
- Configuration options (MaximumLength, ValidCharacters, MaxIterations)
- Edge cases (very short/long IDs, restrictive character sets)
- Error handling (invalid parameters, null values, out-of-range values)

### NuGet Package
The project is configured to automatically generate NuGet packages on build (`GeneratePackageOnBuild` is enabled). Package output can be found in `src/PrettyId/bin/Release/`.

## Architecture

### Core Components

**IdGenerator (src/PrettyId/PrettyId.cs)**
- Instance-based class with multiple generation methods:
  - `Generate()`: Produces IDs by filtering base64-encoded GUIDs/random bytes through a character set
  - `GenerateUrlSafe()`: Produces URL-safe IDs (alphanumeric + `-_.~`)
  - `GenerateKSortable()`: Produces timestamp-prefixed IDs for chronological database sorting
  - `GenerateAndEncodeBase64()`: Generates a random string and base64-encodes it
- Uses an iterative approach (max 256 iterations by default) to build IDs character-by-character from multiple GUIDs/random sources until reaching the desired length
- Configurable via instance properties:
  - `ValidCharacters`: HashSet<char> for O(1) lookup performance (defaults to `EnglishAlphanumeric`)
  - `MaximumLength`: Default length when not specified (default: 32)
  - `MaxIterations`: Max GUID/random generation attempts (default: 64, max: 256)
  - `UseCryptographicRng`: Use RandomNumberGenerator instead of GUIDs (default: false)

**DefaultCharacterSets (src/PrettyId/DefaultCharacterSets.cs)**
- Provides predefined character sets:
  - `EnglishAlphanumeric`: a-z, A-Z, 0-9 (default)
  - `Base64`: Alphanumeric + '+', '/', '='
  - `USKeyboard`: All typeable characters on US keyboard
  - `HexadecimalUppercase`: 0-9, A-F
  - `HexadecimalLowercase`: 0-9, a-f
  - `UrlSafe`: Alphanumeric + '-', '_', '.', '~' (RFC 3986 unreserved)

### Generation Algorithm

The `Generate()` method works by:
1. Converting GUIDs (or RandomNumberGenerator bytes) to base64 strings as a source of randomness
2. Filtering characters through `ValidCharacters` (HashSet for O(1) lookup)
3. Building the result with `StringBuilder` for efficient string concatenation
4. Repeating with new random sources until desired length is reached (up to `MaxIterations`)
5. Throwing `InvalidOperationException` if unable to generate within iteration limit

The `GenerateKSortable()` method:
1. Generates a base36-encoded Unix timestamp (milliseconds)
2. Appends an underscore separator
3. Calls `Generate()` to fill the remainder with random characters

The `GenerateAndEncodeBase64()` method:
1. Adjusts target length to be divisible by 4 (base64 requirement)
2. Generates a random string using `Generate()`
3. Base64-encodes that string to produce valid base64 output

### Multi-Targeting

The library targets multiple frameworks:
- .NET Standard 2.0, 2.1
- .NET Framework 4.61, 4.62, 4.8
- .NET 6.0, 8.0

This broad targeting ensures compatibility across legacy and modern .NET applications.

## Key Implementation Details

- **No uniqueness guarantee**: While based on GUIDs/secure random with astronomically low collision probability, the library cannot guarantee never issuing the same ID twice
- **Performance optimizations**:
  - Uses `StringBuilder` instead of string concatenation (O(n) vs O(n²))
  - Uses `HashSet<char>` for ValidCharacters instead of arrays (O(1) vs O(n) lookup)
  - Generates IDs in microseconds on modern hardware
- **Performance characteristics**: Inversely proportional to character set size - smaller character sets require more iterations
- **Prefix handling**: When a prefix is provided, the `maxLen` parameter includes the prefix length in the total
- **K-sortable constraints**: `GenerateKSortable()` adds timestamp prefix, reducing available space for random portion
- **Base64 constraints**: `GenerateAndEncodeBase64()` requires `maxLen` to be at least 4 characters longer than the prefix due to base64 encoding overhead
- **Thread safety**: Instance methods are thread-safe for reading (generation), but not for writing (modifying properties)
- **Warning system**: Warns to console when character sets have fewer than 16 characters, as this may cause performance issues

## Code Style

- **No var keyword**: Always use explicit types (e.g., `string id` instead of `var id`, `IdGenerator generator` instead of `var generator`)
- **No tuples**: Avoid tuple types and deconstruction
- **StringBuilder usage**: For string building operations
- **HashSet usage**: For character set lookups
- **Clear error messages**: All exceptions include contextual information (e.g., actual values, parameter names)

## Testing

The Test project (`src/Test/Test.csproj`) contains a comprehensive automated test suite that validates every scenario:
- Returns exit code 0 if all tests pass, 1 if any fail
- Prints PASS/FAIL for each individual test
- Provides a summary with total/passed/failed counts and percentages
- Tests cover all generation methods, configuration options, edge cases, and error handling
