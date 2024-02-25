namespace SegregatedStorage.UnitTests.Repositories.Mongo;

public class MongoSetupTests
{
	[Fact]
	public void AddMongoFileRepository_DoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddMongoFileRepository<int>("mongodb://mongo:secret@127.0.0.1:32781/", "collection", key => $"db {key}");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IKeyServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = serviceLocator.GetService(42);
		Assert.IsType<MongoFileRepository>(service);
	}
}