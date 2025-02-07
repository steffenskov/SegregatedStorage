using NSubstitute;
using SegregatedStorage.Utilities;

namespace SegregatedStorage.UnitTests.Services;

public class DeletionBackgroundServiceTests
{
	[Fact]
	public async Task DeleteFromRepositoriesAsync_NoRepositories_DoesNothing()
	{
		// Arrange
		var getServicesCalled = false;
		var repositoryLocator = Substitute.For<IServiceLocator<int, IFileRepository>>();
		repositoryLocator.When(locator => locator.GetServices()).Do(_ => { getServicesCalled = true; });
		repositoryLocator.GetServices().Returns(Array.Empty<(int, IFileRepository)>());
		var storageProviderLocator = Substitute.For<IServiceLocator<int, IStorageProvider>>();
		var service = new FakeDeletionBackgroundService(repositoryLocator, storageProviderLocator);

		// Act
		await service.InvokeDeleteFromRepositoriesAsync();

		// Assert
		Assert.True(getServicesCalled);
	}

	[Fact]
	public async Task DeleteFromRepositoriesAsync_NoFiles_DoesNothing()
	{
		// Arrange
		var getForDeletionCalled = false;
		var repository = Substitute.For<IFileRepository>();
		repository.When(rep => rep.GetForDeletionAsync()).Do(_ => { getForDeletionCalled = true; });
		repository.GetForDeletionAsync().Returns(ValueTask.FromResult(Enumerable.Empty<StoredFile>()));

		var repositoryLocator = Substitute.For<IServiceLocator<int, IFileRepository>>();
		repositoryLocator.GetServices().Returns([(42, repository)]);
		var storageProviderLocator = Substitute.For<IServiceLocator<int, IStorageProvider>>();
		var service = new FakeDeletionBackgroundService(repositoryLocator, storageProviderLocator);

		// Act
		await service.InvokeDeleteFromRepositoriesAsync();

		// Assert
		Assert.True(getForDeletionCalled);
	}

	[Fact]
	public async Task DeleteFromRepositoriesAsync_Files_DeletesThem()
	{
		// Arrange
		const int key1 = 42;
		const int key2 = 1337;
		var file = StoredFile.Create(Guid.NewGuid(), "hello world", "plain/text");
		var storageProviderDeleteCalled = new Dictionary<int, bool>();
		var repositoryDeleteCalled = new Dictionary<int, bool>();

		var repository1 = Substitute.For<IFileRepository>();
		repository1.GetForDeletionAsync().Returns([file]);
		repository1.When(repo => repo.DeleteAsync(file.Id)).Do(_ => { repositoryDeleteCalled[key1] = true; });

		var repository2 = Substitute.For<IFileRepository>();
		repository2.GetForDeletionAsync().Returns([file]);
		repository2.When(repo => repo.DeleteAsync(file.Id)).Do(_ => { repositoryDeleteCalled[key2] = true; });

		var storageProvider1 = Substitute.For<IStorageProvider>();
		storageProvider1.When(provider => provider.DeleteAsync(FilePathGenerator.GenerateFilePath(file.Id))).Do(_ => { storageProviderDeleteCalled[key1] = true; });

		var storageProvider2 = Substitute.For<IStorageProvider>();
		storageProvider2.When(provider => provider.DeleteAsync(FilePathGenerator.GenerateFilePath(file.Id))).Do(_ => { storageProviderDeleteCalled[key2] = true; });

		var repositoryLocator = Substitute.For<IServiceLocator<int, IFileRepository>>();
		repositoryLocator.GetServices().Returns([(key1, repository1), (key2, repository2)]);
		var storageProviderLocator = Substitute.For<IServiceLocator<int, IStorageProvider>>();
		storageProviderLocator.GetService(key1).Returns(storageProvider1);
		storageProviderLocator.GetService(key2).Returns(storageProvider2);
		var service = new FakeDeletionBackgroundService(repositoryLocator, storageProviderLocator);

		// Act
		await service.InvokeDeleteFromRepositoriesAsync();

		// Assert
		Assert.True(storageProviderDeleteCalled[key1]);
		Assert.True(storageProviderDeleteCalled[key2]);
		Assert.True(repositoryDeleteCalled[key1]);
		Assert.True(repositoryDeleteCalled[key2]);
	}
}

file class FakeDeletionBackgroundService : DeletionBackgroundService<int>
{
	public FakeDeletionBackgroundService(IServiceLocator<int, IFileRepository> repositoryLocator, IServiceLocator<int, IStorageProvider> storageProviderLocator) : base(
		repositoryLocator, storageProviderLocator)
	{
	}

	public async Task InvokeDeleteFromRepositoriesAsync()
	{
		await DeleteFromRepositoriesAsync(default);
	}
}