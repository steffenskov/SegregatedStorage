using System.Net;
using Microsoft.Azure.Cosmos;
using SegregatedStorage.Aggregates;
using SegregatedStorage.ValueObjects;

namespace SegregatedStorage.Repositories;

internal class CosmosFileRepository : IFileRepository
{
	private static readonly PartitionKey _partitionKey = new(Consts.PartitionKey);
	private readonly Container _container;

	public CosmosFileRepository(Container container)
	{
		_container = container;
	}

	public async ValueTask PersistAsync(StoredFile storedFile, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(storedFile);
		var item = new CosmosStoredFile(storedFile);
		await _container.UpsertItemAsync(item, cancellationToken: cancellationToken);
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
		var query = new QueryDefinition($"SELECT * from c WHERE c.id IN ({string.Join(", ", ids.Select(id => $"'{id}'"))})"); // Essentially SQL Injection, but with guids which are safe

		return await QueryItemsAsync(query, cancellationToken);
	}

	public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		try
		{
			await _container.DeleteItemAsync<StoredFile>(id.ToString(), _partitionKey, cancellationToken: cancellationToken);
		}
		catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}
	}

	public async ValueTask<IEnumerable<StoredFile>> GetForDeletionAsync(CancellationToken cancellationToken = default)
	{
		var query = new QueryDefinition("SELECT * FROM c WHERE c.state = @state")
			.WithParameter("@state", FileState.Deleting);

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