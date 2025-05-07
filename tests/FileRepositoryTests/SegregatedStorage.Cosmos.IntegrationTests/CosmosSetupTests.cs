namespace SegregatedStorage.Cosmos.IntegrationTests;

[Collection(nameof(ConfigurationCollection))]
public class CosmosSetupTests
{
	private readonly string _connectionString;

	public CosmosSetupTests(ContainerFixture fixture)
	{
		_connectionString = fixture.ConnectionString;
	}

	[Fact]
	public async Task AddCosmosFileRepository_SegregatedDatabaseDoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddCosmosFileRepository<int>(_connectionString, "mycol", key => $"db_{key}");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IAsyncServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = await serviceLocator.GetServiceAsync(42);
		Assert.IsType<CosmosFileRepository>(service);
	}

	[Fact]
	public async Task AddCosmosFileRepository_SegregatedContainerDoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddCosmosFileRepository<int>(_connectionString, key => "mycol_" + key, "db");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IAsyncServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = await serviceLocator.GetServiceAsync(42);
		Assert.IsType<CosmosFileRepository>(service);
	}
}