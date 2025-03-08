namespace SegregatedStorage.Services;

internal class StorageService<TKey> : IStorageService<TKey>
	where TKey : notnull
{
	private readonly IAsyncServiceLocator<TKey, IFileRepository> _repositoryLocator;
	private readonly IAsyncServiceLocator<TKey, IStorageProvider> _storageProviderLocator;

	public StorageService(IAsyncServiceLocator<TKey, IFileRepository> repositoryLocator, IAsyncServiceLocator<TKey, IStorageProvider> storageProviderLocator)
	{
		_storageProviderLocator = storageProviderLocator;
		_repositoryLocator = repositoryLocator;
	}

	public async ValueTask<StoredFile> UploadAsync(TKey key, string filename, string mimeType, Stream data, CancellationToken cancellationToken = default)
	{
		var id = Guid.NewGuid();
		return await UploadAsync(key, id, filename, mimeType, data, cancellationToken);
	}

	public async ValueTask<StoredFile> UploadAsync(TKey key, Guid id, string filename, string mimeType, Stream data, CancellationToken cancellationToken = default)
	{
		var repository = await _repositoryLocator.GetServiceAsync(key, cancellationToken);
		var storageProvider = await _storageProviderLocator.GetServiceAsync(key, cancellationToken);
		var file = StoredFile.Create(id, filename, mimeType);
		await repository.PersistAsync(file, cancellationToken);

		try
		{
			await storageProvider.UploadAsync(FilePathGenerator.GenerateFilePath(file.Id), data, cancellationToken);
			file = file.Uploaded();
			await repository.PersistAsync(file, CancellationToken.None);
			return file;
		}
		catch (OperationCanceledException)
		{
			await repository.DeleteAsync(file.Id, CancellationToken.None);
			throw;
		}
	}

	public async ValueTask<(StoredFile File, Stream Data)> DownloadAsync(TKey key, Guid id, CancellationToken cancellationToken = default)
	{
		var repository = await _repositoryLocator.GetServiceAsync(key, cancellationToken);
		var storageProvider = await _storageProviderLocator.GetServiceAsync(key, cancellationToken);
		var file = await repository.GetAsync(id, cancellationToken);

		if (file is null || file.State == FileState.Deleting)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}

		if (file.State == FileState.AwaitingUpload)
		{
			throw new InvalidOperationException($"File with id {id} has not uploaded its data yet");
		}

		var stream = await storageProvider.DownloadAsync(FilePathGenerator.GenerateFilePath(id), cancellationToken);

		return (file, stream);
	}

	public async ValueTask DeleteAsync(TKey key, Guid id, CancellationToken cancellationToken = default)
	{
		var repository = await _repositoryLocator.GetServiceAsync(key, cancellationToken);
		var file = await repository.GetAsync(id, cancellationToken);
		if (file is null || file.State == FileState.Deleting)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}

		var deletedFile = file.Delete();
		await repository.PersistAsync(deletedFile, cancellationToken);
	}

	public async ValueTask<IDictionary<Guid, StoredFile>> GetManyAsync(TKey key, IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
	{
		var repository = await _repositoryLocator.GetServiceAsync(key, cancellationToken);
		var result = await repository.GetManyAsync(ids, cancellationToken);
		return result.Where(file => file.State != FileState.Deleting)
			.ToDictionary(file => file.Id, file => file);
	}

	public async ValueTask<StoredFile> GetAsync(TKey key, Guid id, CancellationToken cancellationToken = default)
	{
		var repository = await _repositoryLocator.GetServiceAsync(key, cancellationToken);
		var file = await repository.GetAsync(id, cancellationToken);

		if (file is null || file.State == FileState.Deleting)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}

		return file;
	}

	public async ValueTask<StoredFile> RenameAsync(TKey key, Guid id, string filename, CancellationToken cancellationToken = default)
	{
		var repository = await _repositoryLocator.GetServiceAsync(key, cancellationToken);
		var file = await repository.GetAsync(id, cancellationToken);

		if (file is null || file.State == FileState.Deleting)
		{
			throw new FileNotFoundException($"File not found with id {id}");
		}

		var renamedFile = file.Rename(filename);

		await repository.PersistAsync(renamedFile, cancellationToken);
		return renamedFile;
	}
}