
![GitHub License](https://img.shields.io/github/license/raphaelschmitz00/BaseEcs)
![NuGet Version](https://img.shields.io/nuget/v/EcsBase?logo=nuget)

# EcsBase

A minimalist starting point for using ECS in C#.


## 🧠 The Idea
ECS libraries tend to come with a list of features,  
which is what the library maintainer(s) deem useful.  
However, these lists varying greatly between libraries shows there is no "silver bullet".

Instead, the idea for EcsBase is:
1. Add this library via NuGet and start developing.
2. Use it through a wrapper or a replacement for `EcsService` and start developing.
3. When reaching EcsBase's limits, copy its files and quickly understand and modify them.

Now, let's go through those steps!



## 📦 1. Add the Package
```
dotnet add package EcsBase
```



## 🚀 2. Use the Library

### 2.1 Add the EcsBase namespace 🏷️
```csharp
using EcsBase;
```


### 2.2 Create a world 🌍
```csharp
var ecsService = EcsService<int>.Create();
```
This creates a new `EcsService`, which contains a `World`, a `ChangeBuffer`, and a `QueryExecutor`.  
We are using `int` as the key type here - this is what is usually called the "entity".


### 2.3 Add an Entity 👤
```csharp
ecsService
    .ChangeBuffer
    .AddEntity(7)
    .WithComponent(new Transform2D { Position = new Vector2(1.0f, 3.0f) });

ecsService.ChangeBuffer.ApplyChanges();
```

**Identifiers**  
Note that you are responsible for providing a unique key for each entity  
(hardcoded to 7 in this example).

**Component Types**  
Components can be any class or struct, really.  
Of course, structs would have a performance advantage,  
but see the section about performance further down.

**ChangeBuffer**  
The actual lists of components should not be changed while iterating over it.  
⚠️ **Note: There are no built-in protections that prevent you from doing this!** ⚠️  
That is why `ChangeBuffer` comes with an `ApplyChanges()` method.  
This will _actually_ apply any changes that are requested via its other methods.

So, call `ChangeBuffer.ApplyChanges()` at the beginning or end of a frame and you're good.


### 2.4 Execute a query 🚀
```csharp
public class MyQuery : IQuery<int, Transform2D>
{
	public void Execute(Span<int> e, Span<Transform2D> component1Span)
	{
		for (int i = 0; i < e.Length; i++)
		{
			component1Span[i].Position.X++;
		}
	}
}

var myQuery = new MyQuery();
ecsService.Queries.Execute<MyQuery, Transform2D, ComponentB>(myQuery);
```

The first generic type parameter of `IQuery` is the key type,  
the rest defines which components an entity needs to have to be part of the query. 



## 🏗️ 3. Modify the Library

### 🗂️ File Overview
- Component state
  - `Chunk` holds a somewhat memory-optimized collection of components
  - `ChunkList` holds multiple chunks, and creates new chunks when necessary
  - `Archetype` is basically just a collection of types
  - `World` manages a `ChunkList` per `Archetype`, and keeps track of entities
- Editing entities
    - `EntityBuilder` is a fluent interface to create entities
    - `ChangeBuffer` helps with delaying changes to the start or end of a frame
- Querying
  - `IQuery` contains 9 variations (1-9 component parameters) of the query interface
  - `QueryExecutor` contains 9 methods to run these queries
- And finalle
  - `EcsService` ties it all together as a common entry point


## "Features"

### 🦺 Safe
No `unsafe` code. I have seen this advertised as a bonus.  
To be honest, I do not know where that would come into play -  
but yeah, there is no direct memory-wrangling involved.

### 🤏 Minimalism
The entire code is in 9 files, all but one smaller than 200 lines of code.  
The library is only 35Kb in size (not that this would be unexpected for, well,  9 files).

It can arguably not even be called an "Entity/Component/System" library,  
because there is no system (`IQuery` hardly qualifies).  
It's not an ECS, just an "EC".

Strike that, an entity is not provided either.  
You can freely choose what to use as entity via the `TKey` generic parameter.  
`int`, `long`, `Guid` - or just any class.  
So it's not an ECS, just a "C"... wouldn't that be a confusing name.



### 🏎️ Performance
Performance is somewhat of an afterthought here.  

Components inside a `Chunk` are stored contiguously _per component_,  
and which `ChunkList`s are applicable for a specific combination of components is cached.

I'm simply not making the type of game where I need to process 100 000 entities per frame.

Anyway, 3 different queries over different combinations of components on 100 000 entities  
take about 100 microseconds on my machine. (See the `EcsBase.Benchmarks` project.)
