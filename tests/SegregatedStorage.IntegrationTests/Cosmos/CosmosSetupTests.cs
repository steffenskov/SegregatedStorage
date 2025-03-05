namespace SegregatedStorage.IntegrationTests.Cosmos;

[Collection(nameof(ConfigurationCollection))]
public class CosmosSetupTests : BaseTests
{
	private readonly string _connectionString;

	public CosmosSetupTests(ContainerFixture fixture) : base(fixture)
	{
		_connectionString = fixture.CosmosConnectionString;
	}

	[Fact]
	public async Task AddCosmosFileRepository_DoesNotExist_IsAdded()
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
}