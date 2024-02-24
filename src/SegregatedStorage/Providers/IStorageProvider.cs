namespace SegregatedStorage.Providers;

public interface IStorageProvider
{
	ValueTask UploadAsync(string filePath, Stream data, CancellationToken cancellationToken = default);
	ValueTask DeleteAsync(string filePath, CancellationToken cancellationToken = default);
	ValueTask<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default);
}