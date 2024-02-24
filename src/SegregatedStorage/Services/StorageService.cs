namespace SegregatedStorage.Services;

internal class StorageService : IStorageService
{
	private readonly IFileRepository _repository;
	private readonly IStorageProvider _storageProvider;

	public StorageService(IStorageProvider storageProvider, IFileRepository repository)
	{
		_storageProvider = storageProvider;
		_repository = repository;
	}

	public async ValueTask<Guid?> UploadAsync(string filename, string mimeType, Stream data, CancellationToken cancellationToken = default)
	{
		var file = FileAggregate.Create(filename, mimeType);
		await _repository.PersistAsync(file, cancellationToken);

		try
		{
			await _storageProvider.UploadAsync(FilePathGenerator.CreateFilePath(file.Id), data, cancellationToken);
			file = file.Uploaded();
			await _repository.PersistAsync(file, CancellationToken.None);
			return file.Id;
		}
		catch (OperationCanceledException)
		{
			await _repository.DeleteAsync(file.Id, CancellationToken.None);
			return null;
		}
	}

	public async ValueTask<(FileAggregate File, Stream Data)> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var file = await _repository.GetAsync(id, cancellationToken);

		if (file is null || file.State == FileState.Deleting)
			throw new FileNotFoundException($"File not found with id {id}");

		if (file.State == FileState.AwaitingUpload)
			throw new InvalidOperationException($"File with id {id} has not uploaded its data yet");

		var stream = await _storageProvider.DownloadAsync(FilePathGenerator.CreateFilePath(id), cancellationToken);

		return (file, stream);
	}

	public async ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		var file = await _repository.GetAsync(id, cancellationToken);
		if (file is null || file.State == FileState.Deleting)
			throw new FileNotFoundException($"File not found with id {id}");

		var deletedFile = file.Delete();
		await _repository.PersistAsync(deletedFile, cancellationToken);
	}
}