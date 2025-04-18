using SegregatedStorage.ValueObjects;

namespace SegregatedStorage.UnitTests.Services;

public class StorageServiceTests
{
	private readonly AsyncServiceLocator<int, IFileRepository> _repositoryLocator;
	private readonly StorageService<int> _service;

	public StorageServiceTests()
	{
		_repositoryLocator = new AsyncServiceLocator<int, IFileRepository>(_ => new InMemoryFileRepository());
		var storageProviderLocator = new AsyncServiceLocator<int, IStorageProvider>(_ => new InMemoryStorageProvider());
		_service = new StorageService<int>(_repositoryLocator, storageProviderLocator);
	}

	[Fact]
	public async Task UploadAsync_DoesNotExist_IsUploaded()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);

		// Act
		var uploadedFile = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Assert
		var (file, data) = await _service.DownloadAsync(42, uploadedFile.Id, TestContext.Current.CancellationToken);
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
		await _service.UploadAsync(42, id, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Assert
		var (file, data) = await _service.DownloadAsync(42, id, TestContext.Current.CancellationToken);
		Assert.NotNull(file);
		Assert.NotNull(data);
	}

	[Fact]
	public async Task UploadAsync_FileNameAlreadyExist_IsUploaded()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var id = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Act
		var id2 = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Assert
		Assert.NotEqual(id, id2);
	}

	[Fact]
	public async Task DownloadAsync_DoesNotExist_Throws()
	{
		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DownloadAsync(42, Guid.NewGuid(), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DownloadAsync_StateIsDeleting_Throws()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DownloadAsync(42, file.Id, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DownloadAsync_StateIsAwaiting_Throws()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain");
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.DownloadAsync(42, file.Id, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DownloadAsync_Exists_IsDownloaded()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var uploadedFile = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Act
		var (file, data) = await _service.DownloadAsync(42, uploadedFile.Id, TestContext.Current.CancellationToken);
		Assert.Equal("hello.txt", file.FileName);
		Assert.Equal("text/plain", file.MimeType);
		Assert.Equal(FileState.Available, file.State);
		Assert.Equal(uploadedFile, file);

		using var ms2 = new MemoryStream();
		await data.CopyToAsync(ms2, TestContext.Current.CancellationToken);
		var fetchedBytes = ms2.ToArray();
		Assert.True(bytes.SequenceEqual(fetchedBytes));
	}

	[Fact]
	public async Task DeleteAsync_DoesNotExist_Throws()
	{
		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DeleteAsync(42, Guid.NewGuid(), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DeleteAsync_StateIsDeleting_Throws()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.DeleteAsync(42, file.Id, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task DeleteAsync_Exists_IsDeleted()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var uploadedFile = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Act
		var ex = await Record.ExceptionAsync(async () => await _service.DeleteAsync(42, uploadedFile.Id, TestContext.Current.CancellationToken));

		// Assert
		Assert.Null(ex);
	}

	[Fact]
	public async Task GetAsync_DoesNotExist_Throws()
	{
		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.GetAsync(42, Guid.NewGuid(), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task GetAsync_StateIsDeleting_Throws()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.GetAsync(42, file.Id, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task GetAsync_StateIsAwaiting_IsReturned()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain");
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		var fetched = await _service.GetAsync(42, file.Id, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(file, fetched);
	}

	[Fact]
	public async Task GetAsync_Exists_IsReturned()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var file = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);

		// Act
		var fetched = await _service.GetAsync(42, file.Id, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(file, fetched);
	}

	[Fact]
	public async Task GetManyAsync_DoesNotExist_ReturnsEmpty()
	{
		// Act
		var result = await _service.GetManyAsync(42, [Guid.NewGuid()], TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetManyAsync_StateIsDeleting_ReturnsEmpty()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		var result = await _service.GetManyAsync(42, [file.Id], TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetManyAsync_StateIsAwaiting_IsReturned()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain");
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);


		// Act
		var result = await _service.GetManyAsync(42, [file.Id], TestContext.Current.CancellationToken);

		// Assert
		Assert.Contains(file, result.Values);
	}

	[Fact]
	public async Task GetManyAsync_Exists_IsReturned()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var file = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);
		var file2 = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain");
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file2, TestContext.Current.CancellationToken);

		// Act
		var result = await _service.GetManyAsync(42, [file.Id, file2.Id], TestContext.Current.CancellationToken);

		// Assert
		Assert.Contains(file, result.Values);
		Assert.Contains(file2, result.Values);
	}

	[Fact]
	public async Task RenameAsync_DoesNotExist_Throws()
	{
		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.RenameAsync(42, Guid.NewGuid(), "world.txt", TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task RenameAsync_StateIsDeleting_Throws()
	{
		// Arrange
		var file = StoredFile.Create(Guid.NewGuid(), "hello.txt", "text/plain").Delete();
		await (await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken)).PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await _service.RenameAsync(42, file.Id, "world.txt", TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task RenameAsync_WhitespaceFilename_Throws()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var file = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		var ex = await Assert.ThrowsAsync<ArgumentException>(async () => await _service.RenameAsync(42, file.Id, "   ", TestContext.Current.CancellationToken));

		Assert.Contains("filename cannot be null or whitespace", ex.Message);
	}

	[Fact]
	public async Task RenameAsync_ValidFilename_Renamed()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var file = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		var result = await _service.RenameAsync(42, file.Id, "world.txt", TestContext.Current.CancellationToken);

		// Assert
		var fetched = await _service.GetAsync(42, file.Id, TestContext.Current.CancellationToken);
		Assert.Equal("world.txt", result.FileName);
		Assert.Equal("world.txt", fetched.FileName);
	}

	[Fact]
	public async Task RenameAsync_ValidFilenameWithNewExtension_DoesNotChangeMimeType()
	{
		// Arrange
		var bytes = "Hello world"u8.ToArray();
		var ms = new MemoryStream(bytes);
		var file = await _service.UploadAsync(42, "hello.txt", "text/plain", ms, TestContext.Current.CancellationToken);
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		var result = await _service.RenameAsync(42, file.Id, "world.jpg", TestContext.Current.CancellationToken);

		// Assert
		var fetched = await _service.GetAsync(42, file.Id, TestContext.Current.CancellationToken);
		Assert.Equal("world.jpg", result.FileName);
		Assert.Equal("world.jpg", fetched.FileName);
		Assert.Equal("text/plain", fetched.MimeType);
	}
}