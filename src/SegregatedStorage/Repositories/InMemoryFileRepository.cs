using System.Collections.Concurrent;

namespace SegregatedStorage.Repositories;

internal class InMemoryFileRepository : IFileRepository
{
	private readonly ConcurrentDictionary<Guid, FileAggregate> _files = new();

	public ValueTask PersistAsync(FileAggregate file, CancellationToken cancellationToken = default)
	{
		_files[file.Id] = file;
		return ValueTask.CompletedTask;
	}

	public ValueTask<FileAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (_files.TryGetValue(id, out var result))
			return ValueTask.FromResult(result);

		throw new FileNotFoundException($"File not found with id {id}");
	}

	public ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default)
	{
		if (!_files.TryRemove(id, out _))
			throw new FileNotFoundException($"File not found with id {id}");

		return ValueTask.CompletedTask;
	}

	public ValueTask<IEnumerable<FileAggregate>> GetForDeletionAsync(CancellationToken cancellationToken = default)
	{
		var result = _files.Values.Where(file => file.State == FileState.Deleting);

		return ValueTask.FromResult(result);
	}
}