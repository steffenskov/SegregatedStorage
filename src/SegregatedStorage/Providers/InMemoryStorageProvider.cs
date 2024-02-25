using System.Collections.Concurrent;

namespace SegregatedStorage.Providers;

internal class InMemoryStorageProvider : IStorageProvider
{
	private readonly ConcurrentDictionary<string, byte[]> _files = new();

	public async ValueTask UploadAsync(string filePath, Stream data, CancellationToken cancellationToken = default)
	{
		if (_files.TryGetValue(filePath, out _))
			throw new ArgumentException($"File already exists at path {filePath}", nameof(filePath));

		using var ms = new MemoryStream();
		await data.CopyToAsync(ms, cancellationToken);
		ms.Seek(0, SeekOrigin.Begin);
		_files[filePath] = ms.ToArray();
	}

	public ValueTask DeleteAsync(string filePath, CancellationToken cancellationToken = default)
	{
		if (!_files.TryRemove(filePath, out _))
			throw new FileNotFoundException($"File not found with path {filePath}");

		return ValueTask.CompletedTask;
	}

	public ValueTask<Stream> DownloadAsync(string filePath, CancellationToken cancellationToken = default)
	{
		if (!_files.TryGetValue(filePath, out var data))
			throw new FileNotFoundException($"File not found with path {filePath}");

		var ms = new MemoryStream(data);
		ms.Seek(0, SeekOrigin.Begin);
		return ValueTask.FromResult((Stream)ms);
	}
}