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
public IEnumerable<MyMessage> GetMessages(string[] ids)
{
  _someClient.Open();
  var buffer = new CallbackYielder.Buffer<MyMessage>(push =>
      _someClient.MethodWithCallback(ids, annoyingCallback: newMessage => push(newMessage))
    .StopIf.NoPushSince(seconds: 3600)
    .Finally(() => _someClient.Close());
    
  foreach(var item in buffer.Enumerate())
    yield return item;
}
```
