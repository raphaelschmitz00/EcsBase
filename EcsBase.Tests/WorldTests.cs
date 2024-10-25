using FluentAssertions;

namespace EcsBase.Tests;



public class WorldTests
{
	private readonly World<string> _world = new();


	[Fact]
	public void AddNewEntity_ShouldAddEntitySuccessfully()
	{
		var components = new Dictionary<Type, object>
		{
			{ typeof(int), 42 },
			{ typeof(string), "Test" }
		};

		Action act = () => _world.AddNewEntity("entity1", components);

		act.Should().NotThrow();
		_world.GetFittingChunkLists(new[] { typeof(int), typeof(string) }).Should().NotBeEmpty();
	}


	[Fact]
	public void AddNewComponent_ShouldAddComponentToEntity()
	{
		var components = new Dictionary<Type, object> { { typeof(int), 42 } };
		_world.AddNewEntity("entity1", components);

		_world.AddNewComponent("entity1", "NewComponent");

		var chunkList = _world.GetFittingChunkLists(new[] { typeof(int), typeof(string) });
		chunkList.Should().NotBeEmpty();
	}


	[Fact]
	public void DestroyComponent_ShouldRemoveComponentFromEntity()
	{
		var components = new Dictionary<Type, object>
		{
			{ typeof(int), 42 },
			{ typeof(string), "Test" }
		};
		_world.AddNewEntity("entity1", components);

		_world.DestroyComponent("entity1", typeof(string));

		var chunkList = _world.GetFittingChunkLists(new[] { typeof(int) });
		chunkList.Should().NotBeEmpty();

		chunkList = _world.GetFittingChunkLists(new[] { typeof(string) });
		chunkList.Should().BeEmpty();
	}


	[Fact]
	public void DestroyEntity_ShouldRemoveEntityCompletely()
	{
		var components = new Dictionary<Type, object> { { typeof(int), 42 } };
		_world.AddNewEntity("entity1", components);

		_world.DestroyEntity("entity1");

		var chunkList = _world.GetFittingChunkLists(new[] { typeof(int) });
		chunkList.Should().BeEmpty();
	}


	[Fact]
	public void GetFittingChunkLists_ShouldReturnMatchingChunkLists()
	{
		var components1 = new Dictionary<Type, object> { { typeof(int), 42 } };
		var components2 = new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "Test" } };

		_world.AddNewEntity("entity1", components1);
		_world.AddNewEntity("entity2", components2);

		var chunkList = _world.GetFittingChunkLists(new[] { typeof(int) });
		chunkList.Should().HaveCountGreaterThan(0);

		chunkList = _world.GetFittingChunkLists(new[] { typeof(int), typeof(string) });
		chunkList.Should().HaveCount(1);
	}


	[Fact]
	public void InvalidateCachedQueries_ShouldRemoveInvalidQueries()
	{
		var components1 = new Dictionary<Type, object> { { typeof(int), 42 } };
		var components2 = new Dictionary<Type, object> { { typeof(int), 42 }, { typeof(string), "Test" } };

		_world.AddNewEntity("entity1", components1);
		_world.AddNewEntity("entity2", components2);

		var chunkList = _world.GetFittingChunkLists(new[] { typeof(int) });
		chunkList.Should().HaveCountGreaterThan(0);

		_world.DestroyEntity("entity2");

		chunkList = _world.GetFittingChunkLists(new[] { typeof(int), typeof(string) });
		chunkList.Should().BeEmpty();
	}
}
