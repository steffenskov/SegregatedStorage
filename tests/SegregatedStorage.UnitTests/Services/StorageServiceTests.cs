using SegregatedStorage.ValueObjects;

namespace SegregatedStorage.UnitTests.Services;

public class StorageServiceTests
{
	private readonly KeyServiceLocator<int, IFileRepository> _repositoryLocator;
	private readonly StorageService<int> _service;

	public StorageServiceTests()
	{
		_repositoryLocator = new KeyServiceLocator<int, IFileRepository>(_ => new InMemoryFileRepository());
		var storageProviderLocator = new KeyServiceLocator<int, IStorageProvider>(_ => new InMemoryStorageProvider());
		_service = new StorageService<int>(_repositoryLocator, storageProviderLocator);
	}

	[Fact]
	public async Task UploadAsync_DoesNotExist_IsUploaded()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);

		// Act
		var uploadedFile = await _service.UploadAsync(42, "hello.txt", "text/plain", ms);

		// Assert
		var (file, data) = await _service.DownloadAsync(42, uploadedFile.Id);
		Assert.NotNull(file);
		Assert.NotNull(data);
	}

	[Fact]
	public async Task UploadAsync_WithPredefinedId_IsUploadedWithSpecifiedId()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);

		// Act
		var id = Guid.NewGuid();
		await _service.UploadAsync(42, id, "hello.txt", "text/plain", ms);

		// Assert
		var (file, data) = await _service.DownloadAsync(42, id);
		Assert.NotNull(file);
		Assert.NotNull(data);
	}

	[Fact]
	public async Task UploadAsync_FileNameAlreadyExist_IsUploaded()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var id = await _service.UploadAsync(42, "hello.txt", "text/plain", ms);

		// Act
		var id2 = await _service.UploadAsync(42, "hello.txt", "text/plain", ms);

		// Assert
		Assert.NotEqual(id, id2);
	}

	[Fact]
	public async Task DownloadAsync_DoesNotExist_Throws()
	{
		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DownloadAsync(42, Guid.NewGuid()));
	}

	[Fact]
	public async Task DownloadAsync_StateIsDeleting_Throws()
	{
		// Arrange
		var file = FileAggregate.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await _repositoryLocator.GetService(42).PersistAsync(file);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DownloadAsync(42, file.Id));
	}

	[Fact]
	public async Task DownloadAsync_StateIsAwaiting_Throws()
	{
		// Arrange
		var file = FileAggregate.Create(Guid.NewGuid(), "hello.txt", "text/plain");
		await _repositoryLocator.GetService(42).PersistAsync(file);

		// Act && Assert
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DownloadAsync(42, file.Id));
	}

	[Fact]
	public async Task DownloadAsync_Exists_IsDownloaded()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var uploadedFile = await _service.UploadAsync(42, "hello.txt", "text/plain", ms);

		// Act
		var (file, data) = await _service.DownloadAsync(42, uploadedFile.Id);
		Assert.Equal("hello.txt", file.FileName);
		Assert.Equal("text/plain", file.MimeType);
		Assert.Equal(FileState.Available, file.State);
		Assert.Equal(uploadedFile, file);

		using var ms2 = new MemoryStream();
		await data.CopyToAsync(ms2);
		var fetchedBytes = ms2.ToArray();
		Assert.True(bytes.SequenceEqual(fetchedBytes));
	}

	[Fact]
	public async Task DeleteAsync_DoesNotExist_Throws()
	{
		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DeleteAsync(42, Guid.NewGuid()));
	}

	[Fact]
	public async Task DeleteAsync_StateIsDeleting_Throws()
	{
		// Arrange
		var file = FileAggregate.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await _repositoryLocator.GetService(42).PersistAsync(file);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DeleteAsync(42, file.Id));
	}

	[Fact]
	public async Task DeleteAsync_Exists_IsDeleted()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var uploadedFile = await _service.UploadAsync(42, "hello.txt", "text/plain", ms);

		// Act
		var ex = await Record.ExceptionAsync(async () => await _service.DeleteAsync(42, uploadedFile.Id));

		// Assert
		Assert.Null(ex);
	}
}