using Microsoft.Extensions.Hosting;

namespace SegregatedStorage.Services;

internal class DeletionBackgroundService<TKey> : BackgroundService
	where TKey : notnull
{
	private readonly IKeyServiceLocator<TKey, IFileRepository> _repositoryLocator;
	private readonly IKeyServiceLocator<TKey, IStorageProvider> _storageProviderLocator;

	public DeletionBackgroundService(IKeyServiceLocator<TKey, IFileRepository> repositoryLocator, IKeyServiceLocator<TKey, IStorageProvider> storageProviderLocator)
	{
		_repositoryLocator = repositoryLocator;
		_storageProviderLocator = storageProviderLocator;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await DeleteFromRepositoriesAsync(stoppingToken);

			await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
		}
	}

	protected async Task DeleteFromRepositoriesAsync(CancellationToken cancellationToken)
	{
		foreach (var (key, repository) in _repositoryLocator.GetServices())
		{
			var files = await repository.GetForDeletionAsync(cancellationToken);

			foreach (var file in files)
				await DeleteFileAsync(key, repository, file, cancellationToken);
		}
	}

	private async Task DeleteFileAsync(TKey key, IFileRepository repository, StoredFile storedFile, CancellationToken cancellationToken)
	{
		var deleted = await DeleteFromStorageProviderAsync(key, storedFile, cancellationToken);

		if (deleted) await repository.DeleteAsync(storedFile.Id, cancellationToken);
	}

	private async Task<bool> DeleteFromStorageProviderAsync(TKey key, StoredFile storedFile, CancellationToken cancellationToken)
	{
		var storageProvider = _storageProviderLocator.GetService(key);
		try
		{
			await storageProvider.DeleteAsync(FilePathGenerator.GenerateFilePath(storedFile.Id), cancellationToken);
			return true;
		}
		catch (FileNotFoundException)
		{
			return true;
		}
		catch
		{
			return false;
		}
	}
}