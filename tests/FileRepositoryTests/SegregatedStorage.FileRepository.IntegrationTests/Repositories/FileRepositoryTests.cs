using SegregatedStorage.Aggregates;
using SegregatedStorage.Repositories;
using SegregatedStorage.Services;

namespace SegregatedStorage.FileRepository.IntegrationTests.Repositories;

public abstract class FileRepositoryTests
{
	private readonly IAsyncServiceLocator<int, IFileRepository> _repositoryLocator;

	protected FileRepositoryTests(IAsyncServiceLocator<int, IFileRepository> repositoryLocator)
	{
		_repositoryLocator = repositoryLocator;
	}

	[Fact]
	public async Task PersistAsync_Valid_IsPersisted()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");

		// Act
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Assert
		var ex = await Record.ExceptionAsync(async () => await repository.GetAsync(file.Id, TestContext.Current.CancellationToken));

		Assert.Null(ex);
	}

	[Fact]
	public async Task PersistAsync_IdAlreadyExists_IsOverwritten()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		var updatedFile = file.Uploaded();

		// Act
		await repository.PersistAsync(updatedFile, TestContext.Current.CancellationToken);

		// Assert
		var fetched = await repository.GetAsync(file.Id, TestContext.Current.CancellationToken);
		Assert.NotEqual(file, updatedFile);
		Assert.Equal(updatedFile, fetched);
	}

	[Fact]
	public async Task GetAsync_DoesNotExist_Throws()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await repository.GetAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task GetAsync_ExistOnOtherKey_Throws()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var otherRepository = await _repositoryLocator.GetServiceAsync(43, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await otherRepository.GetAsync(file.Id, TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task GetAsync_Exist_IsRetrieved()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		var fetched = await repository.GetAsync(file.Id, TestContext.Current.CancellationToken);

		// Assert
		Assert.Equal(file, fetched);
	}

	[Fact]
	public async Task GetManyAsync_DoesNotExist_ReturnsEmpty()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);

		// Act
		var result = await repository.GetManyAsync([Guid.NewGuid(), Guid.NewGuid()], TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetManyAsync_ExistsOnOtherKey_ReturnsEmpty()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var otherRepository = await _repositoryLocator.GetServiceAsync(43, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		var result = await otherRepository.GetManyAsync([file.Id], TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetManyAsync_SomeExist_AreReturned()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		var file2 = StoredFile.Create(Guid.NewGuid(), "text.txt", "text/plain");
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);
		await repository.PersistAsync(file2, TestContext.Current.CancellationToken);

		// Act
		var result = (await repository.GetManyAsync([file.Id, file2.Id, Guid.NewGuid()], TestContext.Current.CancellationToken)).ToList();

		// Assert
		Assert.Equal(2, result.Count());
		Assert.Contains(file, result);
		Assert.Contains(file2, result);
	}

	[Fact]
	public async Task DeleteAsync_Exist_IsDeleted()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file, TestContext.Current.CancellationToken);

		// Act
		await repository.DeleteAsync(file.Id, TestContext.Current.CancellationToken);

		// Assert
		var ex = await Record.ExceptionAsync(async () => await repository.GetAsync(file.Id, TestContext.Current.CancellationToken));
		Assert.IsType<FileNotFoundException>(ex);
	}

	[Fact]
	public async Task DeleteAsync_DoesNotExist_Throws()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task GetForDeletionAsync_NoneExist_ReturnsEmptyCollection()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);

		// Act
		var files = await repository.GetForDeletionAsync(TestContext.Current.CancellationToken);

		// Assert
		Assert.Empty(files);
	}

	[Fact]
	public async Task GetForDeletionAsync_SomeExist_ReturnsOnlyDeleted()
	{
		// Arrange
		var repository = await _repositoryLocator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var file1 = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		var file2 = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg").Delete();
		var file3 = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg").Delete();
		await repository.PersistAsync(file1, TestContext.Current.CancellationToken);
		await repository.PersistAsync(file2, TestContext.Current.CancellationToken);
		await repository.PersistAsync(file3, TestContext.Current.CancellationToken);

		// Act
		var files = (await repository.GetForDeletionAsync(TestContext.Current.CancellationToken)).ToList();

		// Assert
		Assert.DoesNotContain(file1, files);
		Assert.Contains(file2, files);
		Assert.Contains(file3, files);
	}
}