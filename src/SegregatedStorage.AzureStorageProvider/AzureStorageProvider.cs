using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SegregatedStorage.Providers;

namespace SegregatedStorage;

internal class AzureStorageProvider : IStorageProvider
{
	private readonly BlobContainerClient _client;

	public AzureStorageProvider(BlobServiceClient blobServiceClient, string blobContainerName)
	{
		_client = blobServiceClient.GetBlobContainerClient(blobContainerName);
		_client.CreateIfNotExists();
	}

	public async ValueTask UploadAsync(string filePath, Stream data, CancellationToken cancellationToken = default)
	{
		try
		{
			await _client.UploadBlobAsync(filePath, data, cancellationToken);
		}
		catch (RequestFailedException ex) when (ex.Status == 409)
		{
			throw new ArgumentException($"File already exists at path {filePath}", nameof(filePath));
		}
	}

	public async ValueTask DeleteAsync(string filePath, CancellationToken cancellationToken = default)
	{
		var result = await _client.DeleteBlobIfExistsAsync(filePath, DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
		if (!result.HasValue || !result.Value)
			throw new FileNotFoundException($"File not found with path {filePath}");
	}

	public async ValueTask<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
	{
		var blob = _client.GetBlobClient(filePath);
		try
		{
			var result = await blob.DownloadAsync(cancellationToken);
			if (!result.HasValue)
				throw new FileNotFoundException($"File not found with path {filePath}");

			return result.Value.Content;
		}
		catch (RequestFailedException ex) when (ex.Status == 404)
		{
			throw new FileNotFoundException($"File not found with path {filePath}");
		}
	}
}