namespace SegregatedStorage.UnitTests.Providers.InMemory;

public class InMemoryStorageProviderTests
{
	[Fact]
	public async Task UploadAsync_NewFilePath_IsCreated()
	{
		// Arrange
		var filePath = Guid.NewGuid().ToString();
		var provider = new InMemoryStorageProvider();
		var bytes = "Hello world"u8.ToArray();
		var stream = new MemoryStream(bytes);

		// Act
		await provider.UploadAsync(filePath, stream, TestContext.Current.CancellationToken);

		// Assert
		var fetched = await provider.DownloadAsync(filePath, TestContext.Current.CancellationToken);
		var ms = new MemoryStream();
		await fetched.CopyToAsync(ms, TestContext.Current.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);
		var fetchedBytes = ms.ToArray();

		Assert.True(bytes.SequenceEqual(fetchedBytes));
	}

	[Fact]
	public async Task UploadAsync_FileExists_Throws()
	{
		// Arrange
		var filePath = Guid.NewGuid().ToString();
		var provider = new InMemoryStorageProvider();
		var bytes = "Hello world"u8.ToArray();
		var stream = new MemoryStream(bytes);
		await provider.UploadAsync(filePath, stream, TestContext.Current.CancellationToken);
		stream.Seek(0, SeekOrigin.Begin);

		// Act && Assert
		var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await provider.UploadAsync(filePath, stream, TestContext.Current.CancellationToken));
		Assert.Contains($"File already exists at path {filePath}", ex.Message);
	}

	[Fact]
	public async Task DeleteAsync_FileDoesNotExist_Throws()
	{
		// Arrange
		var filePath = Guid.NewGuid().ToString();
		var provider = new InMemoryStorageProvider();

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.DeleteAsync(filePath, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DeleteAsync_Exist_IsDeleted()
	{
		// Arrange
		var filePath = Guid.NewGuid().ToString();
		var provider = new InMemoryStorageProvider();
		var bytes = "Hello world"u8.ToArray();
		var stream = new MemoryStream(bytes);
		await provider.UploadAsync(filePath, stream, TestContext.Current.CancellationToken);

		// Act
		var ex = await Record.ExceptionAsync(async () => await provider.DeleteAsync(filePath, TestContext.Current.CancellationToken));

		// Assert
		Assert.Null(ex);
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.DownloadAsync(filePath, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DownloadAsync_FileDoesNotExist_Throws()
	{
		// Arrange
		var filePath = Guid.NewGuid().ToString();
		var provider = new InMemoryStorageProvider();

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await provider.DownloadAsync(filePath, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DownloadAsync_Exists_IsDownloaded()
	{
		// Arrange
		var filePath = Guid.NewGuid().ToString();
		var provider = new InMemoryStorageProvider();
		var bytes = "Hello world"u8.ToArray();
		var stream = new MemoryStream(bytes);
		await provider.UploadAsync(filePath, stream, TestContext.Current.CancellationToken);

		// Act
		var result = await provider.DownloadAsync(filePath, TestContext.Current.CancellationToken);

		// Assert
		using var ms = new MemoryStream();
		await result.CopyToAsync(ms, TestContext.Current.CancellationToken);
		ms.Seek(0, SeekOrigin.Begin);
		var fetchedBytes = ms.ToArray();

		Assert.True(bytes.SequenceEqual(fetchedBytes));
	}
}