namespace SegregatedStorage.Mongo.IntegrationTests;

[Collection(nameof(ConfigurationCollection))]
public class MongoSetupTests
{
	private readonly string _connectionString;

	public MongoSetupTests(ContainerFixture fixture)
	{
		_connectionString = fixture.ConnectionString;
	}

	[Fact]
	public async Task AddMongoFileRepository_SegregatedDatabaseDoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddMongoFileRepository<int>(_connectionString, "mycol", key => $"db_{key}");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IAsyncServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = await serviceLocator.GetServiceAsync(42);
		Assert.IsType<MongoFileRepository>(service);
	}

	[Fact]
	public async Task AddMongoFileRepository_SegregatedCollectionDoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddMongoFileRepository<int>(_connectionString, key => "mycol_" + key, "db");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IAsyncServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = await serviceLocator.GetServiceAsync(42);
		Assert.IsType<MongoFileRepository>(service);
	}
}