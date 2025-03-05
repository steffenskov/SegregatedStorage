using System.Net;
using Microsoft.Azure.Cosmos;
using SegregatedStorage.Aggregates;
using SegregatedStorage.Repositories;
using SegregatedStorage.ValueObjects;

namespace SegregatedStorage;

internal class CosmosFileRepository : IFileRepository
{
	private static readonly PartitionKey _partitionKey = new($"/{nameof(StoredFile.State).ToLower()}");
	private readonly Container _container;

	public CosmosFileRepository(CosmosClient client, string databaseId, string containerId)
	{
		_container = client.GetContainer(databaseId, containerId);
	}

	public async ValueTask PersistAsync(StoredFile storedFile, CancellationToken cancellationToken = default)
	{
		await _container.UpsertItemAsync(storedFile, _partitionKey, cancellationToken: cancellationToken);
	}

	public async ValueTask<StoredFile> GetAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			var response = await _container.ReadItemAsync<StoredFile>(id.ToString(), _partitionKey, cancellationToken: cancellationToken);
			return response.Resource;
		}
		catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}
	}

	public async ValueTask<IEnumerable<StoredFile>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
	{
		var query = new QueryDefinition("SELECT * from c WHERE c.id IN (@ids)")
			.WithParameter("@ids", ids.Select(id => id.ToString()));

		return await QueryItemsAsync(query, cancellationToken);
	}

	public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			await _container.DeleteItemAsync<StoredFile>(id.ToString(), new PartitionKey("/id"), cancellationToken: cancellationToken);
		}
		catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}
	}

	public async ValueTask<IEnumerable<StoredFile>> GetForDeletionAsync(CancellationToken cancellationToken = default)
	{
		var query = new QueryDefinition("SELECT * FROM c WHERE c.state = @state")
			.WithParameter("@state", FileState.Deleting.ToString());

		return await QueryItemsAsync(query, cancellationToken);
	}

	private async Task<IEnumerable<StoredFile>> QueryItemsAsync(QueryDefinition query, CancellationToken cancellationToken)
	{
		var result = new List<StoredFile>();
		using var iterator = _container.GetItemQueryIterator<StoredFile>(query);
		while (iterator.HasMoreResults)
		{
			foreach (var file in await iterator.ReadNextAsync(cancellationToken))
			{
				result.Add(file);
			}
		}

		return result;
	}
}