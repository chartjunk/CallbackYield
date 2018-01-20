# CallbackYielder
[![NuGet](https://img.shields.io/nuget/v/CallbackYielder.svg)]() [![NuGet](https://img.shields.io/nuget/dt/CallbackYielder.svg)]()

Enumerate items from an arbitrary callback method. Fluent API and all...

# Getting started
NuGet package (https://www.nuget.org/packages/CallbackYielder/) is available via NPM:
```
install-package CallbackYielder
```

# Tl;dr
Items are pushed into a buffer and yielded out of the buffer;
*TODO: Document*

# Example

```C#
using CallbackYielder;

public List<MyMessage> GetMessages(string[] ids)
{
  var buffer = CallbackYielderBuilder
    .Buffer<Message>(push =>
      _client.MethodWithACallbackParameter(ids, annoyingCallback: newMessage => push(newMessage))
    .StopAfter.NoYieldSince(seconds: 3600)
    .Finally(() => _client.Close())
    .Build();
    
  return buffer.Enumerate().ToList();
}
```
