using SegregatedStorage.Providers;

namespace SegregatedStorage;

public class AzureStorageProvider : IStorageProvider
{
	public ValueTask UploadAsync(string filePath, Stream data, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public ValueTask DeleteAsync(string filePath, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public ValueTask<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
