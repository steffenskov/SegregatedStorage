namespace SegregatedStorage.Repositories;

public interface IFileRepository
{
	ValueTask PersistAsync(StoredFile storedFile, CancellationToken cancellationToken = default);
	ValueTask<StoredFile> GetAsync(Guid id, CancellationToken cancellationToken = default);
	ValueTask<IEnumerable<StoredFile>> GetManyAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
	ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default);
	ValueTask<IEnumerable<StoredFile>> GetForDeletionAsync(CancellationToken cancellationToken = default);
}