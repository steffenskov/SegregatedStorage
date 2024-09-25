using MongoDB.Driver;
using SegregatedStorage.Aggregates;
using SegregatedStorage.Repositories;
using SegregatedStorage.ValueObjects;

namespace SegregatedStorage;

internal class MongoFileRepository : IFileRepository
{
	private readonly IMongoCollection<StoredFile> _collection;

	public MongoFileRepository(IMongoDatabase db, string collectionName)
	{
		_collection = db.GetCollection<StoredFile>(collectionName);
		CreateIndex(builder => builder.Descending(e => e.State));
	}

	public async ValueTask PersistAsync(StoredFile storedFile, CancellationToken cancellationToken = default)
	{
		await _collection.ReplaceOneAsync(f => f.Id == storedFile.Id, storedFile, new ReplaceOptions
		{
			IsUpsert = true
		}, cancellationToken: cancellationToken);
	}

	public async ValueTask<StoredFile> GetAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var cursor = await _collection.FindAsync(f => f.Id == id, cancellationToken: cancellationToken);
		var result = await cursor.FirstOrDefaultAsync(cancellationToken);

		return result ?? throw new FileNotFoundException($"File not found with id {id}");
	}

	public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteOneAsync(f => f.Id == id, cancellationToken: cancellationToken);
		if (result.DeletedCount == 0)
			throw new FileNotFoundException($"File not found with id {id}");
	}

	public async ValueTask<IEnumerable<StoredFile>> GetForDeletionAsync(CancellationToken cancellationToken = default)
	{
		var cursor = await _collection.FindAsync(f => f.State == FileState.Deleting, cancellationToken: cancellationToken);

		return await cursor.ToListAsync(cancellationToken);
	}

	private void CreateIndex(Func<IndexKeysDefinitionBuilder<StoredFile>, IndexKeysDefinition<StoredFile>> configureIndex, CreateIndexOptions? options = default)
	{
		_collection.Indexes.CreateOne(new CreateIndexModel<StoredFile>(configureIndex(Builders<StoredFile>.IndexKeys), options));
	}
}
