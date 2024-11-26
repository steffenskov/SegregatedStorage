namespace SegregatedStorage.Services;

public interface IStorageService<in TKey>
	where TKey : notnull
{
	/// <summary>
	///     Uploads a file to the storage service and returns its Id.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="filename">Pretty name of the file, will be used when downloading the file.</param>
	/// <param name="mimeType">MimeType of the file, will be used when downloading the file.</param>
	/// <param name="data">The actual data contents of the file.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>Metadata for the file uploaded.</returns>
	ValueTask<StoredFile> UploadAsync(TKey key, string filename, string mimeType, Stream data, CancellationToken cancellationToken = default);

	/// <summary>
	///     Uploads a file to the storage service using the given id.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="id">Id of the file</param>
	/// <param name="filename">Pretty name of the file, will be used when downloading the file.</param>
	/// <param name="mimeType">MimeType of the file, will be used when downloading the file.</param>
	/// <param name="data">The actual data contents of the file.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>Metadata for the file uploaded.</returns>
	ValueTask<StoredFile> UploadAsync(TKey key, Guid id, string filename, string mimeType, Stream data, CancellationToken cancellationToken = default);

	/// <summary>
	///     Downloads a file from the storage service.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="id">Id of the file to download, will throw FileNotFoundException if no such file exists.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>Stream with the actual data contents of the file.</returns>
	ValueTask<(StoredFile File, Stream Data)> DownloadAsync(TKey key, Guid id, CancellationToken cancellationToken = default);

	/// <summary>
	///     Deletes a file from the storage service.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="id">Id of the file to download, will throw FileNotFoundException if no such file exists.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	ValueTask DeleteAsync(TKey key, Guid id, CancellationToken cancellationToken = default);

	/// <summary>
	///     Fetches the metadata for a bunch of files.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="ids">Ids of the files to fetch.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>Dictionary of file metadata</returns>
	ValueTask<IDictionary<Guid, StoredFile>> GetManyAsync(TKey key, IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

	/// <summary>
	///     Fetches the metadata for a single file.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="id">Id of the file to fetch, will throw FileNotFoundException if no such file exists.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>File metadata</returns>
	ValueTask<StoredFile> GetAsync(TKey key, Guid id, CancellationToken cancellationToken = default);

	/// <summary>
	///     Renames the file in its metadata, CANNOT change mimetype of the file even if a new file extension is used.
	/// </summary>
	/// <param name="key">Key used for data segregation</param>
	/// <param name="id">Id of the file to fetch, will throw FileNotFoundException if no such file exists.</param>
	/// <param name="filename">New filename for the file.</param>
	/// <param name="cancellationToken">CancellationToken, can be omitted</param>
	/// <returns>File metadata</returns>
	ValueTask<StoredFile> RenameAsync(TKey key, Guid id, string filename, CancellationToken cancellationToken = default);
}