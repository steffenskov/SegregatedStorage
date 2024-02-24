namespace SegregatedStorage.Services;

public interface IStorageService
{
	/// <summary>
	///     Uploads a file to the storage service and returns its Id.
	/// </summary>
	/// <param name="filename">Pretty name of the file, will be used when downloading the file.</param>
	/// <param name="mimeType">MimeType of the file, will be used when downloading the file.</param>
	/// <param name="data">The actual data contents of the file.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>Id of the file uploaded.</returns>
	ValueTask<Guid?> UploadAsync(string filename, string mimeType, Stream data, CancellationToken cancellationToken = default);

	/// <summary>
	///     Downloads a file from the storage service.
	/// </summary>
	/// <param name="id">Id of the file to download, will throw FileNotFoundException if no such file exists.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>Stream with the actual data contents of the file.</returns>
	ValueTask<(FileAggregate File, Stream Data)> DownloadAsync(Guid id, CancellationToken cancellationToken = default);

	/// <summary>
	///     Deletes a file from the storage service.
	/// </summary>
	/// <param name="id">Id of the file to download, will throw FileNotFoundException if no such file exists.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	ValueTask DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}