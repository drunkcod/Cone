# Cone vNext

## What's New

### Check.With 
Check.With aims to streamline scenarios where it's desierable to check 
multiple related properties on a given return value. This often happens 
around repositories, builders or other provider like objects. 

For illustrative purposes assume there's a reason for us having:
`Thing` that holds a `Value` it's a string of great importance.
`ThingProvider` can get us a `Thing`. 

What we've settled on is that we want to check if the `Thing`
returned from the `ThingProvider` is a thing with properties we deem proper.

The base implementation of this check is simply
```csharp
    var things = new ThingProvider();
    Check.That(
        () => things.GetThing().Value.Length == 13,
        () => things.GetThing().Value.EndsWith("!"));
```
Sample output being akin to:
```
  -> things.GetThing().Value.Length == 13
  Expected: 13
  But was:  11
things.GetThing().Value.EndsWith("!")
  Expected: a string ending with "!"
  But was:  "Hello World"
```

This works well in most circumstances and is straightforward although a tad
repetitive. One way to cut down the duplication while keeping the safeguards
around getting things is to exploit Check.That actual chaining
```csharp
    var things = new ThingProvider();
    var thing = (Thing)Check.That(() => things.GetThing() != null);
    Check.That(
        () => thing.Value.Length == 13,
        () => thing.Value.EndsWith("!"));
```
Here we'll imediatly break if there's a problem gettin the thing, and provide
reasonable context in that case. We can the use the local thing for checking.
This makes the core checks much more to the point and avoids double execution
but comes at the cost of a slight loss in context.

Failure output now looks like this:
```
  -> thing.Value.Length == 13
  Expected: 13
  But was:  11
thing.Value.EndsWith("!")
  Expected: a string ending with "!"
  But was:  "Hello World"
```

If the loss of the source of "thing" is relvant or not is ofcourse contextual
often it's acceptable, but the implementation is a bit of an aquired taste.

Thus Check.With was born. The above scenario can now simply be stated as:
```csharp
    var things = new ThingProvider();
    Check.With(() => things.GetThing())
    .That(
        thing => thing.Value.Length == 13,
        thing => thing.Value.EndsWith("!"));
```

With establish a context for the following checks and ensures it's non 
nullness. The signature of That changes slightly to now take a single 
parameter that will be the result from a single shared invocation of the
lambda given to With. 

Upon failure the complete context is given to make it easer to piece
togheter what went avry: 
```
 given things.GetThing() ->
 thing.Value.Length == 13
  Expected: 13
  But was:  11
thing.Value.EndsWith("!")
  Expected: a string ending with "!"
  But was:  "Hello World"
```

We know know where thing (or whatever we decide to call it) came from, thus
we avoid duplication but still keep the full context on display for easy
resolution. Also. no odd casts from object or strange return values...


## Fixed Bugs
* Fixed an issue with MethodSpy and null targets.
