namespace SegregatedStorage.Services;

internal class StorageService<TKey> : IStorageService<TKey>
	where TKey : notnull
{
	private readonly IKeyServiceLocator<TKey, IFileRepository> _repositoryLocator;
	private readonly IKeyServiceLocator<TKey, IStorageProvider> _storageProviderLocator;

	public StorageService(IKeyServiceLocator<TKey, IFileRepository> repositoryLocator, IKeyServiceLocator<TKey, IStorageProvider> storageProviderLocator)
	{
		_storageProviderLocator = storageProviderLocator;
		_repositoryLocator = repositoryLocator;
	}

	public async ValueTask<Guid?> UploadAsync(TKey key, string filename, string mimeType, Stream data, CancellationToken cancellationToken = default)
	{
		var repository = _repositoryLocator.GetService(key);
		var storageProvider = _storageProviderLocator.GetService(key);
		var file = FileAggregate.Create(filename, mimeType);
		await repository.PersistAsync(file, cancellationToken);

		try
		{
			await storageProvider.UploadAsync(FilePathGenerator.CreateFilePath(file.Id), data, cancellationToken);
			file = file.Uploaded();
			await repository.PersistAsync(file, CancellationToken.None);
			return file.Id;
		}
		catch (OperationCanceledException)
		{
			await repository.DeleteAsync(file.Id, CancellationToken.None);
			return null;
		}
	}

	public async ValueTask<(FileAggregate File, Stream Data)> DownloadAsync(TKey key, Guid id, CancellationToken cancellationToken = default)
	{
		var repository = _repositoryLocator.GetService(key);
		var storageProvider = _storageProviderLocator.GetService(key);
		var file = await repository.GetAsync(id, cancellationToken);

		if (file is null || file.State == FileState.Deleting)
			throw new FileNotFoundException($"File not found with id {id}");

		if (file.State == FileState.AwaitingUpload)
			throw new InvalidOperationException($"File with id {id} has not uploaded its data yet");

		var stream = await storageProvider.DownloadAsync(FilePathGenerator.CreateFilePath(id), cancellationToken);

		return (file, stream);
	}

	public async ValueTask DeleteAsync(TKey key, Guid id, CancellationToken cancellationToken = default)
	{
		var repository = _repositoryLocator.GetService(key);
		var file = await repository.GetAsync(id, cancellationToken);
		if (file is null || file.State == FileState.Deleting)
			throw new FileNotFoundException($"File not found with id {id}");

		var deletedFile = file.Delete();
		await repository.PersistAsync(deletedFile, cancellationToken);
	}
}