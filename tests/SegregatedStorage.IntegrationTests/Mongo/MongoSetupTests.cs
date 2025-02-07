using SegregatedStorage.IntegrationTests;

namespace SegregatedStorage.IntegrationTests.Mongo;

[Collection(nameof(ConfigurationCollection))]

public class MongoSetupTests : BaseTests
{
	private string _connectionString;

	public MongoSetupTests(ContainerFixture fixture) : base(fixture)
	{
		_connectionString = fixture.MongoConnectionString;
	}

	[Fact]
	public void AddMongoFileRepository_DoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddMongoFileRepository<int>(_connectionString, "mycol", key => $"db_{key}");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = serviceLocator.GetService(42);
		Assert.IsType<MongoFileRepository>(service);
	}
}