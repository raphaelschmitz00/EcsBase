using FluentAssertions;


namespace EcsBase.Tests;



public class ChunkListTests
{
	private readonly Archetype _archetype = new([typeof(int), typeof(string)]);


	[Fact]
	public void ChunkSpan_ShouldReturnSpanOfChunks()
	{
		var chunkList = new ChunkList<string>(_archetype);

		chunkList.GetChunkWithSpace(); // Add a chunk

		chunkList.ChunkSpan.Length.Should().Be(1);
		chunkList.ChunkSpan[0].Should().NotBeNull();
	}


	[Fact]
	public void HasChunks_ShouldBeFalse_WhenNoChunksExist()
	{
		var chunkList = new ChunkList<string>(_archetype);

		chunkList.HasChunks.Should().BeFalse();
	}


	[Fact]
	public void HasChunks_ShouldBeTrue_WhenChunksExist()
	{
		var chunkList = new ChunkList<string>(_archetype);

		chunkList.GetChunkWithSpace();

		chunkList.HasChunks.Should().BeTrue();
	}


	[Fact]
	public void GetChunkWithSpace_ShouldReturnExistingChunk_WhenChunkHasSpace()
	{
		var chunkList = new ChunkList<string>(_archetype);
		var chunk = chunkList.GetChunkWithSpace();

		var chunkWithSpace = chunkList.GetChunkWithSpace();

		chunkWithSpace.Should().BeSameAs(chunk);
	}


	[Fact]
	public void GetChunkWithSpace_ShouldCreateNewChunk_WhenNoChunkHasSpace()
	{
		var chunkList = new ChunkList<string>(_archetype);
		var chunk1 = chunkList.GetChunkWithSpace();

		// Fill the first chunk completely
		for (int i = 0; i < chunk1.MaxSize; i++)
		{
			chunk1.AddEntity($"entity{i}",
				new Dictionary<Type, object> { { typeof(int), i }, { typeof(string), $"value{i}" } });
		}

		// Request a new chunk
		var chunk2 = chunkList.GetChunkWithSpace();

		chunk2.Should().NotBeSameAs(chunk1);
		chunkList.ChunkSpan.Length.Should().Be(2);
	}


	[Fact]
	public void GetChunkWithSpace_ShouldDoubleChunkSize_WhenNoChunkHasSpace()
	{
		var chunkList = new ChunkList<string>(_archetype);

		// Fill the first chunk to force creation of a new one
		var chunk1 = chunkList.GetChunkWithSpace();
		for (int i = 0; i < chunk1.MaxSize; i++)
		{
			chunk1.AddEntity($"entity{i}",
				new Dictionary<Type, object> { { typeof(int), i }, { typeof(string), $"value{i}" } });
		}

		var chunk2 = chunkList.GetChunkWithSpace();

		chunk2.MaxSize.Should().BeGreaterThan(chunk1.MaxSize);
	}


	[Fact]
	public void Remove_ShouldDeleteSpecifiedChunk()
	{
		var chunkList = new ChunkList<string>(_archetype);
		var chunk = chunkList.GetChunkWithSpace();

		chunkList.Remove(chunk);

		chunkList.HasChunks.Should().BeFalse();
		chunkList.ChunkSpan.Length.Should().Be(0);
	}


	[Fact]
	public void Remove_ShouldNotThrow_WhenRemovingNonexistentChunk()
	{
		var chunkList = new ChunkList<string>(_archetype);
		var chunk = chunkList.GetChunkWithSpace();

		chunkList.Remove(chunk);
		Action act = () => chunkList.Remove(chunk);

		act.Should().NotThrow();
	}
}
