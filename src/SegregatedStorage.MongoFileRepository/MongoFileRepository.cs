using MongoDB.Driver;
using SegregatedStorage.Aggregates;
using SegregatedStorage.Repositories;
using SegregatedStorage.ValueObjects;

namespace SegregatedStorage;

internal class MongoFileRepository : IFileRepository
{
	private readonly IMongoCollection<FileAggregate> _collection;

	public MongoFileRepository(IMongoDatabase db, string collectionName)
	{
		_collection = db.GetCollection<FileAggregate>(collectionName);
	}
	
	public async ValueTask PersistAsync(FileAggregate file, CancellationToken cancellationToken = default)
	{
		await _collection.ReplaceOneAsync(f => f.Id == file.Id, file, new ReplaceOptions
		{
			IsUpsert = true
		}, cancellationToken: cancellationToken);
	}

	public async ValueTask<FileAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var cursor = await _collection.FindAsync(f => f.Id == id, cancellationToken: cancellationToken);
		var result = await cursor.FirstOrDefaultAsync(cancellationToken);
		
		return result ?? throw new FileNotFoundException($"File not found with id {id}");
	}

	public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var result= await _collection.DeleteOneAsync(f => f.Id == id, cancellationToken: cancellationToken);
		if (result.DeletedCount == 0)
			throw new FileNotFoundException($"File not found with id {id}");
	}

	public async ValueTask<IEnumerable<FileAggregate>> GetForDeletionAsync(CancellationToken cancellationToken = default)
	{
		var cursor = await _collection.FindAsync(f => f.State == FileState.Deleting, cancellationToken: cancellationToken);

		return await cursor.ToListAsync(cancellationToken);
	}
}
