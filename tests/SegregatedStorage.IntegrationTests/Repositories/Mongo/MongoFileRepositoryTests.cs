namespace SegregatedStorage.IntegrationTests.Repositories.Mongo;

[Collection(nameof(ConfigurationCollection))]
public class MongoFileRepositoryTests : BaseTests
{
	private readonly IKeyServiceLocator<int, IFileRepository> _repositoryLocator;

	public MongoFileRepositoryTests(ContainerFixture fixture) : base(fixture)
	{
		_repositoryLocator = Provider.GetRequiredService<IKeyServiceLocator<int, IFileRepository>>();
	}

	[Fact]
	public async Task PersistAsync_Valid_IsPersisted()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");

		// Act
		await repository.PersistAsync(file);

		// Assert
		var ex = await Record.ExceptionAsync(async () => await repository.GetAsync(file.Id));

		Assert.Null(ex);
	}

	[Fact]
	public async Task PersistAsync_IdAlreadyExists_IsOverwritten()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file);

		var updatedFile = file.Uploaded();

		// Act
		await repository.PersistAsync(updatedFile);

		// Assert
		var fetched = await repository.GetAsync(file.Id);
		Assert.NotEqual(file, updatedFile);
		Assert.Equal(updatedFile, fetched);
	}

	[Fact]
	public async Task GetAsync_DoesNotExist_Throws()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await repository.GetAsync(Guid.NewGuid()));
	}

	[Fact]
	public async Task GetAsync_ExistOnOtherKey_Throws()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var otherRepository = _repositoryLocator.GetService(43);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await otherRepository.GetAsync(file.Id));
	}

	[Fact]
	public async Task GetAsync_Exist_IsRetrieved()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file);

		// Act
		var fetched = await repository.GetAsync(file.Id);

		// Assert
		Assert.Equal(file, fetched);
	}

	[Fact]
	public async Task GetManyAsync_DoesNotExist_ReturnsEmpty()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);

		// Act
		var result = await repository.GetManyAsync([Guid.NewGuid(), Guid.NewGuid()]);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetManyAsync_ExistsOnOtherKey_ReturnsEmpty()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var otherRepository = _repositoryLocator.GetService(43);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file);

		// Act
		var result = await otherRepository.GetManyAsync([file.Id]);

		// Assert
		Assert.Empty(result);
	}

	[Fact]
	public async Task GetManyAsync_SomeExist_AreReturned()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		var file2 = StoredFile.Create(Guid.NewGuid(), "text.txt", "text/plain");
		await repository.PersistAsync(file);
		await repository.PersistAsync(file2);

		// Act
		var result = (await repository.GetManyAsync([file.Id, file2.Id, Guid.NewGuid()])).ToList();

		// Assert
		Assert.Equal(2, result.Count());
		Assert.Contains(file, result);
		Assert.Contains(file2, result);
	}

	[Fact]
	public async Task DeleteAsync_Exist_IsDeleted()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var file = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		await repository.PersistAsync(file);

		// Act
		await repository.DeleteAsync(file.Id);

		// Assert
		var ex = await Record.ExceptionAsync(async () => await repository.GetAsync(file.Id));
		Assert.IsType<FileNotFoundException>(ex);
	}

	[Fact]
	public async Task DeleteAsync_DoesNotExist_Throws()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);

		// Act && Assert
		await Assert.ThrowsAsync<FileNotFoundException>(async () => await repository.DeleteAsync(Guid.NewGuid()));
	}

	[Fact]
	public async Task GetForDeletionAsync_NoneExist_ReturnsEmptyCollection()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);

		// Act
		var files = await repository.GetForDeletionAsync();

		// Assert
		Assert.Empty(files);
	}

	[Fact]
	public async Task GetForDeletionAsync_SomeExist_ReturnsOnlyDeleted()
	{
		// Arrange
		var repository = _repositoryLocator.GetService(42);
		var file1 = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg");
		var file2 = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg").Delete();
		var file3 = StoredFile.Create(Guid.NewGuid(), "image.jpg", "image/jpg").Delete();
		await repository.PersistAsync(file1);
		await repository.PersistAsync(file2);
		await repository.PersistAsync(file3);

		// Act
		var files = (await repository.GetForDeletionAsync()).ToList();

		// Assert
		Assert.DoesNotContain(file1, files);
		Assert.Contains(file2, files);
		Assert.Contains(file3, files);
	}
}