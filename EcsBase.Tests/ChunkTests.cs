using FluentAssertions;

namespace EcsBase.Tests;



public class ChunkTests
{
	private readonly int _chunkSize = 3;
	private readonly Dictionary<Type, int> _typeIndices;
	private readonly Array[] _componentArrays;


	public ChunkTests()
	{
		_typeIndices = new Dictionary<Type, int> { { typeof(int), 0 }, { typeof(string), 1 } };
		_componentArrays = new Array[]
		{
			new int[_chunkSize], // Array for int components
			new string[_chunkSize] // Array for string components
		};
	}


	[Fact]
	public void HasSpace_ShouldBeTrue_WhenEntitiesLessThanSize()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);
		chunk.HasSpace.Should().BeTrue();

		chunk.AddEntity("entity1", new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "hello" } });
		chunk.HasSpace.Should().BeTrue();
	}


	[Fact]
	public void HasEntities_ShouldBeFalse_WhenNoEntitiesAdded()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);
		chunk.HasEntities.Should().BeFalse();
	}


	[Fact]
	public void AddEntity_ShouldIncreaseEntityCount()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);

		chunk.AddEntity("entity1", new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "hello" } });
		chunk.HasEntities.Should().BeTrue();
		chunk.HasSpace.Should().BeTrue();
	}


	[Fact]
	public void AddEntity_ShouldStoreComponentsCorrectly()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);
		var entity = "entity1";
		var components = new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "hello" } };

		chunk.AddEntity(entity, components);

		var intSpan = chunk.GetSpan<int>();
		var stringSpan = chunk.GetSpan<string>();

		intSpan[0].Should().Be(42);
		stringSpan[0].Should().Be("hello");
	}


	[Fact]
	public void RemoveEntity_ShouldDecreaseEntityCount()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);
		var entity = "entity1";

		chunk.AddEntity(entity, new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "hello" } });
		chunk.RemoveEntity(entity);

		chunk.HasEntities.Should().BeFalse();
		chunk.HasSpace.Should().BeTrue();
	}


	[Fact]
	public void RemoveEntity_ShouldClearComponents()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);
		var entity = "entity1";
		chunk.AddEntity(entity, new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "hello" } });

		chunk.RemoveEntity(entity);

		var intSpan = chunk.GetSpan<int>();
		var stringSpan = chunk.GetSpan<string>();

		intSpan.Length.Should().Be(0);
		stringSpan.Length.Should().Be(0);
	}


	[Fact]
	public void GetComponents_ShouldReturnAllComponentsOfEntity()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);
		var entity = "entity1";
		var components = new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "hello" } };

		chunk.AddEntity(entity, components);
		var result = chunk.GetComponents(entity);

		result[typeof(int)].Should().Be(42);
		result[typeof(string)].Should().Be("hello");
	}


	[Fact]
	public void GetEntitySpan_ShouldReturnAllEntities()
	{
		var chunk = new Chunk<string>(_chunkSize, _componentArrays, _typeIndices);

		chunk.AddEntity("entity1", new Dictionary<Type, object> { { typeof(int), 1 }, { typeof(string), "one" } });
		chunk.AddEntity("entity2", new Dictionary<Type, object> { { typeof(int), 2 }, { typeof(string), "two" } });

		var span = chunk.GetEntitySpan();

		span.Length.Should().Be(2);
		span[0].Should().Be("entity1");
		span[1].Should().Be("entity2");
	}
}
