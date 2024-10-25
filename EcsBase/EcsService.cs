namespace EcsBase;



public interface IEcsService<TKey> where TKey : notnull
{
	World<TKey> World { get; }
	ChangeBuffer<TKey> ChangeBuffer { get; }
	QueryExecutor<TKey> Queries { get; }
}



public class EcsService<TKey> : IEcsService<TKey> where TKey : notnull
{
	private EcsService(
		World<TKey> world,
		ChangeBuffer<TKey> changeBuffer,
		QueryExecutor<TKey> queryExecutor
	)
	{
		World = world;
		ChangeBuffer = changeBuffer;
		Queries = queryExecutor;
	}


	public World<TKey> World { get; }
	public ChangeBuffer<TKey> ChangeBuffer { get; }
	public QueryExecutor<TKey> Queries { get; }


	public static EcsService<TKey> Create()
	{
		var world = new World<TKey>();
		var queryExecutor = new QueryExecutor<TKey>(world);
		var changeBuffer = new ChangeBuffer<TKey>(world);
		return new EcsService<TKey>(world, changeBuffer, queryExecutor);
	}
}
