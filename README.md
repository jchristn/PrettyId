![alt tag](https://raw.githubusercontent.com/jchristn/PrettyId/main/PrettyId/Assets/icon.ico)

# PrettyId

[![NuGet Version](https://img.shields.io/nuget/v/PrettyId.svg?style=flat)](https://www.nuget.org/packages/PrettyId/) [![NuGet](https://img.shields.io/nuget/dt/PrettyId.svg)](https://www.nuget.org/packages/PrettyId) 

PrettyId is a simple library for creating uniform IDs similar to those used by Stripe.

Under the hood, PrettyId uses base64 strings of GUIDs to generate a random identifier of the specified length.  You can choose to set a prefix that can be included in the result.

## New in v1.0.0

- Initial release

## Help or feedback

First things first - do you need help or have feedback?  File an issue here!

## Performance and scale

It's pretty quick :)

## Simple Example
```csharp
using PrettyId;

Console.WriteLine(IdGenerator.Generate("data_", 32));
data_0mAg6shuO0GSplEn7GmXR

Console.WriteLine(IdGenerator.Generate(64));
// KL5ULxfC2kujhcMtDKnDKgUAANsBAdESqJBDKIgvLwdxfjo03uJEKLkn9csMt4Q

Console.WriteLine(IdGenerator.Generate());
// 5wS4rcgWk6Tr0CO0sfXgA0NAtlOp60C
```

Want to set the list of valid characters?  
```csharp
IdGenerator.ValidCharacters = new char[] { 'a', 'b', 'c', 'd', ... };
```
**Note** With fewer characters, it will take longer to generate an ID, and there is a mechanism internally to iterate over new GUIDs a maximum of ```MaxIterations``` times.  If this number of iterations is exceeded, an exception will be thrown.  Additionally, only valid base64 characters should be included in the list.  
