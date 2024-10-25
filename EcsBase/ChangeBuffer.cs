namespace EcsBase;



public class ChangeBuffer<TKey>(World<TKey> world)
	where TKey : notnull
{
	private readonly List<EntityBuilder<TKey>> _entityBuilders = [];
	private readonly List<(TKey key, Type componentType)> _componentsToDestroy = [];
	private readonly List<TKey> _entitiesToDestroy = [];


	public IEntityBuilder AddEntity(TKey key)
	{
		var entityBuilder = new EntityBuilder<TKey>(key);
		_entityBuilders.Add(entityBuilder);
		return entityBuilder;
	}


	public void DestroyComponent<TComponent>(TKey entity) =>
		_componentsToDestroy.Add((entity, typeof(TComponent)));


	public void DestroyEntity(TKey entity) => _entitiesToDestroy.Add(entity);


	public void ApplyChanges()
	{
		foreach (var entityBuilder in _entityBuilders)
		{
			entityBuilder.Build(world);
		}

		_entityBuilders.Clear();


		foreach (var tuple in _componentsToDestroy)
		{
			world.DestroyComponent(tuple.key, tuple.componentType);
		}

		_componentsToDestroy.Clear();


		foreach (var key in _entitiesToDestroy)
		{
			world.DestroyEntity(key);
		}

		_entitiesToDestroy.Clear();
	}
}
