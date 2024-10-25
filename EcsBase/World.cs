namespace EcsBase;



public class World<TKey> where TKey : notnull
{
	private readonly Dictionary<Archetype, ChunkList<TKey>> _chunkLists = [];
	private readonly Dictionary<TKey, EntityLocation> _entityLocations = [];

	private readonly Dictionary<HashSet<Type>, List<ChunkList<TKey>>> _cachedQueries = [];


	public event Action<ComponentEventArgs>? ComponentAdded;
	public event Action<ComponentEventArgs>? ComponentRemoving;


	public List<ChunkList<TKey>> GetFittingChunkLists(Type[] componentTypes)
	{
		var existingQueryKey = _cachedQueries.Keys
			.FirstOrDefault(x => x.SetEquals(componentTypes));

		if (existingQueryKey != null) return _cachedQueries[existingQueryKey];

		var newList =
			_chunkLists
				.Where(x => x.Key.HasComponents(componentTypes))
				.Select(x => x.Value)
				.ToList();

		_cachedQueries.Add([.. componentTypes], newList);
		return newList;
	}


	public void AddNewEntity(TKey key, Dictionary<Type, object> components)
	{
		var entityLocation = AddEntity(key, components);
		_entityLocations.Add(key, entityLocation);
		foreach (var component in components)
		{
			ComponentAdded?.Invoke(new ComponentEventArgs(component.Key, component.Value));
		}
	}


	public void AddNewComponent<TComponent>(TKey entity, TComponent component)
		where TComponent : notnull
	{
		var entityLocation = _entityLocations[entity];
		var oldChunk = entityLocation.Chunk;
		var components = oldChunk.GetComponents(entity);
		RemoveEntity(entity, entityLocation);

		components.Add(typeof(TComponent), component);
		_entityLocations[entity] = AddEntity(entity, components);
		ComponentAdded?.Invoke(new ComponentEventArgs(typeof(TComponent), component));
	}


	public void DestroyComponent(TKey entity, Type componentType)
	{
		var entityLocation = _entityLocations[entity];
		var oldChunk = entityLocation.Chunk;
		var components = oldChunk.GetComponents(entity);
		ComponentRemoving?.Invoke(new ComponentEventArgs(componentType, components[componentType]));

		RemoveEntity(entity, entityLocation);

		components.Remove(componentType);
		_entityLocations[entity] = AddEntity(entity, components);
	}


	public void DestroyEntity(TKey entity)
	{
		var entityLocation = _entityLocations[entity];
		var chunk = entityLocation.Chunk;
		var components = chunk.GetComponents(entity);

		foreach (var component in components)
		{
			ComponentRemoving?.Invoke(new ComponentEventArgs(component.Key, component.Value));
		}

		_entityLocations.Remove(entity);
		RemoveEntity(entity, entityLocation);
	}


	private EntityLocation AddEntity(TKey entity, Dictionary<Type, object> components)
	{
		var archetype = GetArchetype(components.Keys);
		var chunkList = _chunkLists[archetype];
		var chunk = chunkList.GetChunkWithSpace();
		chunk.AddEntity(entity, components);
		return new EntityLocation(archetype, chunk);
	}


	private void RemoveEntity(TKey entity, EntityLocation entityLocation)
	{
		var chunk = entityLocation.Chunk;
		chunk.RemoveEntity(entity);
		if (chunk.HasEntities) return;

		var archetype = entityLocation.Archetype;
		var chunkList = _chunkLists[archetype];
		chunkList.Remove(chunk);
		if (chunkList.HasChunks) return;

		_chunkLists.Remove(archetype);
		InvalidateCachedQueries(archetype.ComponentTypes);
	}


	private Archetype GetArchetype(ICollection<Type> componentTypes)
	{
		var existingArchetype = _chunkLists.Keys.FirstOrDefault(
			x => x.HasOnlyComponents(componentTypes));
		if (existingArchetype != null) return existingArchetype;

		var newArchetype = new Archetype([.. componentTypes]);
		var chunkList = new ChunkList<TKey>(newArchetype);
		_chunkLists.Add(newArchetype, chunkList);
		InvalidateCachedQueries(componentTypes);

		return newArchetype;
	}


	private void InvalidateCachedQueries(ICollection<Type> componentTypes)
	{
		var nowInvalidQueriesKeys =
			_cachedQueries
				.Keys
				.Where(x => x.Any(componentTypes.Contains));

		foreach (var invalidQueriesKey in nowInvalidQueriesKeys)
		{
			_cachedQueries.Remove(invalidQueriesKey);
		}
	}



	public class ComponentEventArgs(Type type, object value) : EventArgs
	{
		public Type Type { get; } = type;
		public object Value { get; } = value;
	}



	private readonly record struct EntityLocation(
		Archetype Archetype,
		Chunk<TKey> Chunk
	);
}
