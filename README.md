![alt tag](https://raw.githubusercontent.com/jchristn/PrettyId/main/PrettyId/Assets/icon.ico)

# PrettyId

[![NuGet Version](https://img.shields.io/nuget/v/PrettyId.svg?style=flat)](https://www.nuget.org/packages/PrettyId/) [![NuGet](https://img.shields.io/nuget/dt/PrettyId.svg)](https://www.nuget.org/packages/PrettyId)

**A .NET library for generating human-friendly, customizable random identifiers.**

PrettyId creates clean, readable IDs that are easier to work with than raw GUIDs. Perfect for API keys, short URLs, session tokens, database identifiers, and anywhere you need IDs that are both random and pleasant to look at.

## Quick Start

```bash
dotnet add package PrettyId
```

```csharp
using PrettyId;

// Create a generator instance
IdGenerator generator = new IdGenerator();

// Generate a simple 32-character ID
string id = generator.Generate();
// Example: "5wS4rcgWk6Tr0CO0sfXgA0NAtlOp60C"

// Generate with a prefix
string userId = generator.Generate("user_", 32);
// Example: "user_mAg6shuO0GSplEn7GmX"

// Generate longer IDs for more entropy
string apiKey = generator.Generate("api_", 64);
// Example: "api_KL5ULxfC2kujhcMtDKnDKgUAANsBAdESqJBDKIgvLwdxfjo03uJEK"
```

## Use Cases

### 1. API Keys and Access Tokens

```csharp
IdGenerator generator = new IdGenerator();
string apiKey = generator.Generate("sk_", 48);
// sk_T5wR8pqLmN4vXzKj9hYbGcFdW3sA2eU1iO7uP

string accessToken = generator.Generate("tok_", 64);
// tok_xM9nL2kJ7vC5bH1zT4qR8wG6fD3sY0pE5uA7iO2mK4tN6hB1gF
```

### 2. Database Primary Keys (K-Sortable)

K-sortable IDs include a timestamp prefix, making them sort chronologically in your database:

```csharp
IdGenerator generator = new IdGenerator();

string orderId = generator.GenerateKSortable("order_", 48);
// order_1lx4f0h_kL9mT3pR7wX2vN5bH8qJ1cF

string invoiceId = generator.GenerateKSortable("inv_", 48);
// inv_1lx4f0k_dG4sW9tY6mL2nB7xC3vF5kP
```

The IDs sort chronologically because the timestamp comes first. Great for database indexes and pagination!

### 3. URL-Safe Identifiers

```csharp
IdGenerator generator = new IdGenerator();

// Perfect for short URLs
string shortUrl = generator.GenerateUrlSafe("s_", 16);
// s_kLm3Pq9R

// Share codes
string shareCode = generator.GenerateUrlSafe(null, 12);
// Rf9kLm3Pq9kT
```

### 4. Session IDs

```csharp
IdGenerator generator = new IdGenerator();
generator.UseCryptographicRng = true; // Extra security for session IDs

string sessionId = generator.Generate("sess_", 48);
// sess_xQ9kLm8vT3pR7wX2nB5hJ1cF4gD6sY0uE
```

### 5. Custom Character Sets

Need specific character constraints? Configure your own character set:

```csharp
// Hexadecimal identifiers
IdGenerator hexGenerator = new IdGenerator(DefaultCharacterSets.HexadecimalLowercase);
string hexId = hexGenerator.Generate("key_", 32);
// key_7a3f2d1e9c4b8a6d5e0f1c2a

// Numeric-only codes
char[] numericChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
IdGenerator numericGenerator = new IdGenerator(numericChars);
string code = numericGenerator.Generate(8);
// 72841956
```

### 6. Different Generators for Different ID Types

Create multiple generator instances with different configurations:

```csharp
// API keys: long and cryptographically secure
IdGenerator apiKeyGen = new IdGenerator();
apiKeyGen.UseCryptographicRng = true;

// User IDs: shorter, alphanumeric
IdGenerator userIdGen = new IdGenerator(DefaultCharacterSets.EnglishAlphanumeric, maximumLength: 24);

// Order IDs: k-sortable for database performance
IdGenerator orderIdGen = new IdGenerator();

// Use them independently
string apiKey = apiKeyGen.Generate("sk_", 64);
string userId = userIdGen.Generate("user_", 24);
string orderId = orderIdGen.GenerateKSortable("order_", 48);
```

## Available Character Sets

```csharp
DefaultCharacterSets.EnglishAlphanumeric  // a-z, A-Z, 0-9 (default)
DefaultCharacterSets.Base64               // Alphanumeric + +/=
DefaultCharacterSets.USKeyboard           // All typeable US keyboard characters
DefaultCharacterSets.HexadecimalUppercase // 0-9, A-F
DefaultCharacterSets.HexadecimalLowercase // 0-9, a-f
DefaultCharacterSets.UrlSafe              // Alphanumeric + -_.~
```

## Generation Methods

| Method | Description | Example Output |
|--------|-------------|----------------|
| `Generate()` | Standard ID generation | `5wS4rcgWk6Tr0CO0sfXgA0NAtl` |
| `Generate(prefix, length)` | ID with custom prefix | `user_mAg6shuO0GSplEn7GmX` |
| `GenerateUrlSafe()` | URL-safe characters only | `kLm3Pq-9R_wX.vN` |
| `GenerateKSortable()` | Timestamp + random (database-friendly) | `order_1lx4f0h_kL9mT3pR7wX` |
| `GenerateAndEncodeBase64()` | Random string encoded as base64 | `b64_VGhpc0lzUmFuZG9t` |

## Configuration Options

```csharp
IdGenerator generator = new IdGenerator();

// Set valid characters
generator.ValidCharacters = new HashSet<char>(DefaultCharacterSets.HexadecimalLowercase);

// Set default length
generator.MaximumLength = 48;

// Use cryptographically secure randomness
generator.UseCryptographicRng = true;

// Increase iteration limit for restrictive character sets
generator.MaxIterations = 128;
```

## Important: Uniqueness Considerations

**PrettyId does NOT guarantee uniqueness** by checking against previously generated IDs. It relies on the statistical improbability of collision, similar to GUIDs.

### Best Practices

1. **Use longer IDs for critical systems** (32+ characters)
2. **Add database unique constraints** to catch the rare collision
3. **Use prefixes** to namespace different ID types
4. **For absolute uniqueness**, combine with application-level checking or database sequences

## Performance

PrettyId is designed to be fast:

- ✅ Uses `StringBuilder` for efficient string building
- ✅ Uses `HashSet<char>` for O(1) character lookups
- ✅ Generates IDs in microseconds on modern hardware

**Performance Tips:**

- Larger character sets = faster generation
- Character sets with <16 characters may require multiple iterations
- Use instance-based generators to avoid recreation overhead

## Thread Safety

✅ **Instance-based generators are thread-safe for reading** (all `Generate*` methods)

⚠️ **Not thread-safe for writing** - don't modify properties (`ValidCharacters`, `MaximumLength`, etc.) from multiple threads

```csharp
// Safe: Multiple threads reading
IdGenerator generator = new IdGenerator();
Parallel.For(0, 1000, i => {
    string id = generator.Generate(); // ✅ Thread-safe
});

// Unsafe: Modifying from multiple threads
Parallel.For(0, 10, i => {
    generator.MaximumLength = 32 + i; // ⚠️ Not thread-safe
});

// Safe: Each thread has its own instance
Parallel.For(0, 1000, i => {
    IdGenerator generator = new IdGenerator(); // ✅ Thread-safe
    string id = generator.Generate();
});
```

## Building from Source

```bash
# Build the library
dotnet build src/PrettyId/PrettyId.csproj

# Run the test/demo program
dotnet run --project src/Test/Test.csproj

# Build release (generates NuGet package)
dotnet build src/PrettyId/PrettyId.csproj -c Release
```

## Framework Support

PrettyId supports a wide range of .NET frameworks:

- .NET Standard 2.0, 2.1
- .NET Framework 4.6.1, 4.6.2, 4.8
- .NET 6.0, 8.0+

## Contributing

Contributions are welcome! Please file issues and pull requests at:
https://github.com/jchristn/PrettyId

Ideas for new default character sets? Found a bug? Want to suggest a feature? Open an issue!

## License

MIT License - see LICENSE.md for details

---

**Need help?** File an issue at https://github.com/jchristn/PrettyId/issues
