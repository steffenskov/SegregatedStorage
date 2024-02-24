namespace SegregatedStorage.Repositories;

public interface IFileRepository
{
	ValueTask PersistAsync(FileAggregate file, CancellationToken cancellationToken = default);
	ValueTask<FileAggregate> GetAsync(Guid id, CancellationToken cancellationToken = default);
	ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	ValueTask<IEnumerable<FileAggregate>> GetForDeletionAsync(CancellationToken cancellationToken = default);
}