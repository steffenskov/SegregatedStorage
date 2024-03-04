using Microsoft.Extensions.Hosting;

namespace SegregatedStorage.Services;

internal class DeletionBackgroundService<TKey> : IHostedService
	where TKey : notnull
{
	private readonly IKeyServiceLocator<TKey, IFileRepository> _repositoryLocator;
	private readonly IKeyServiceLocator<TKey, IStorageProvider> _storageProviderLocator;
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _deletionLoop;

	public DeletionBackgroundService(IKeyServiceLocator<TKey, IFileRepository> repositoryLocator, IKeyServiceLocator<TKey, IStorageProvider> storageProviderLocator)
	{
		_repositoryLocator = repositoryLocator;
		_storageProviderLocator = storageProviderLocator;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		if (_deletionLoop is null)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_deletionLoop = DeletionLoop(_cancellationTokenSource.Token);
		}

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_cancellationTokenSource?.Cancel();
		_deletionLoop = null;
		return Task.CompletedTask;
	}

	private async Task DeletionLoop(object? state)
	{
		var cancellationToken = (CancellationToken)state!;

		while (!cancellationToken.IsCancellationRequested)
		{
			await DeleteFromRepositoriesAsync(cancellationToken);

			await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
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