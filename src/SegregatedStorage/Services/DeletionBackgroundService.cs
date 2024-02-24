using Microsoft.Extensions.Hosting;

namespace SegregatedStorage.Services;

internal class DeletionBackgroundService : IHostedService
{
	private readonly IFileRepository _repository;
	private readonly IStorageProvider _storageProvider;
	private CancellationTokenSource? _cancellationTokenSource;
	private Task? _deletionLoop;

	public DeletionBackgroundService(IFileRepository repository, IStorageProvider storageProvider)
	{
		_repository = repository;
		_storageProvider = storageProvider;
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
			var files = await _repository.GetForDeletionAsync(cancellationToken);

			foreach (var file in files)
				await DeleteFileAsync(file, cancellationToken);

			await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
		}
	}

	private async Task DeleteFileAsync(FileAggregate file, CancellationToken cancellationToken)
	{
		var deleted = await DeleteFromStorageProviderAsync(file, cancellationToken);

		if (deleted) await _repository.DeleteAsync(file.Id, cancellationToken);
	}

	private async Task<bool> DeleteFromStorageProviderAsync(FileAggregate file, CancellationToken cancellationToken)
	{
		try
		{
			await _storageProvider.DeleteAsync(FilePathGenerator.CreateFilePath(file.Id), cancellationToken);
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