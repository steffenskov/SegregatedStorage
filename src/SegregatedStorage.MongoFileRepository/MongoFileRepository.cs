using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SegregatedStorage.Aggregates;
using SegregatedStorage.Repositories;
using SegregatedStorage.ValueObjects;

namespace SegregatedStorage;

internal class MongoFileRepository : IFileRepository
{
	private readonly IMongoCollection<StoredFile> _collection;

	static MongoFileRepository()
	{
		try
		{
			BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
		}
		catch (BsonSerializationException)
		{
			// The above will throw if someone else already registered a Guid serializer for Mongo DB
		}
	}

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
		}, cancellationToken);
	}

	public async ValueTask<StoredFile> GetAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var find = _collection.Find(f => f.Id == id);
		var result = await find.FirstOrDefaultAsync(cancellationToken);

		return result ?? throw new FileNotFoundException($"File not found with id {id}");
	}

	public async ValueTask<IEnumerable<StoredFile>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
	{
		var find = _collection.Find(f => ids.Contains(f.Id));
		return await find.ToListAsync(cancellationToken);
	}

	public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteOneAsync(f => f.Id == id, cancellationToken);
		if (result.DeletedCount == 0)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}
	}

	public async ValueTask<IEnumerable<StoredFile>> GetForDeletionAsync(CancellationToken cancellationToken = default)
	{
		var find = _collection.Find(f => f.State == FileState.Deleting);

		return await find.ToListAsync(cancellationToken);
	}

	private void CreateIndex(Func<IndexKeysDefinitionBuilder<StoredFile>, IndexKeysDefinition<StoredFile>> configureIndex, CreateIndexOptions? options = default)
	{
		_collection.Indexes.CreateOne(new CreateIndexModel<StoredFile>(configureIndex(Builders<StoredFile>.IndexKeys), options));
	}
}