namespace SegregatedStorage.IntegrationTests.Mongo;

[Collection(nameof(ConfigurationCollection))]
public class MongoSetupTests : BaseTests
{
	private readonly string _connectionString;

	public MongoSetupTests(ContainerFixture fixture) : base(fixture)
	{
		_connectionString = fixture.MongoConnectionString;
	}

	[Fact]
	public async Task AddMongoFileRepository_DoesNotExist_IsAdded()
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
}