![alt tag](https://raw.githubusercontent.com/jchristn/PrettyId/main/PrettyId/Assets/icon.ico)

# PrettyId

[![NuGet Version](https://img.shields.io/nuget/v/PrettyId.svg?style=flat)](https://www.nuget.org/packages/PrettyId/) [![NuGet](https://img.shields.io/nuget/dt/PrettyId.svg)](https://www.nuget.org/packages/PrettyId) 

PrettyId is a simple library for creating uniform IDs similar to those used by Stripe.

Under the hood, PrettyId uses base64 strings of GUIDs to generate a random identifier of the specified length.  You can choose to set a prefix that can be included in the result.

## New in v1.0.x

- Initial release
- Added support for generating valid base64 values

## Help or feedback

First things first - do you need help or have feedback?  File an issue here!

## Performance and Security

It's pretty quick :)  The shorter the list of allowed characters, the slower it will run.

On security, the library has no way to guarantee that it will *never* issue the same ID twice.  It is however based on GUIDs, which have a ridiculously low theoretical probability of collision (I would encourage you to check out Stephen Cleary's blog post at https://blog.stephencleary.com/2010/11/few-words-on-guids.html)

## Simple Example

```csharp
using PrettyId;

Console.WriteLine(IdGenerator.Generate());
// 32 bytes in length by default
// 5wS4rcgWk6Tr0CO0sfXgA0NAtlOp60C

Console.WriteLine(IdGenerator.Generate("data_", 32));
// 32 bytes in length, including the header
// data_0mAg6shuO0GSplEn7GmXR

Console.WriteLine(IdGenerator.Generate(64));
// 64 bytes in length
// KL5ULxfC2kujhcMtDKnDKgUAANsBAdESqJBDKIgvLwdxfjo03uJEKLkn9csMt4Q

Console.WriteLine(IdGenerator.GenerateBase64("b64_", 64));
// 64 bytes in length, including the header
// b64_TE42UVVmRmltVUtXMExOdStGSE9Pdz09bDlvR1dBSHErVXl4VG1mRndSYWhH
```
Want to set the list of valid characters?  

```csharp
IdGenerator.ValidCharacters = new char[] { 'a', 'b', 'c', 'd', ... };
```
Or use one of the built-in character sets.
```csharp
IdGenerator.ValidCharacters = DefaultCharacterSets.EnglishAlphanumeric;
IdGenerator.ValidCharacters = DefaultCharacterSets.Base64;
IdGenerator.ValidCharacters = DefaultCharacterSets.USKeyboard;
```
Have a recommendation for default character sets?  Please file an issue with the details or submit a PR!
