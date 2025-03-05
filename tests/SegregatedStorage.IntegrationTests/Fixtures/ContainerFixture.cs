using Testcontainers.Azurite;
using Testcontainers.CosmosDb;
using Testcontainers.MongoDb;

namespace SegregatedStorage.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly AzuriteContainer _azureContainer;
	private readonly CosmosDbContainer _cosmosContainer;
	private readonly MongoDbContainer _mongoContainer;

	public ContainerFixture()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithImage("mongo:latest")
			.WithUsername("mongo")
			.WithPassword("secret")
			.Build();

		_azureContainer = new AzuriteBuilder()
			.WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
			.WithCommand("--skipApiVersionCheck")
			.Build();

		_cosmosContainer = new CosmosDbBuilder()
			.Build();
	}

	public string MongoConnectionString => _mongoContainer.GetConnectionString();
	public string AzureConnectionString => _azureContainer.GetConnectionString();
	public string CosmosConnectionString => _cosmosContainer.GetConnectionString();

	public async Task InitializeAsync()
	{
		await _mongoContainer.StartAsync();
		await _azureContainer.StartAsync();
		await _cosmosContainer.StartAsync();
		await Task.CompletedTask;
		//await Task.WhenAll(_mongoContainer.StartAsync(), _azureContainer.StartAsync(), _cosmosContainer.StartAsync());
	}

	public async Task DisposeAsync()
	{
		var mongoResult = _mongoContainer.DisposeAsync();
		var azureResult = _azureContainer.DisposeAsync();
		var cosmosResult = _cosmosContainer.DisposeAsync();

		await mongoResult;
		await azureResult;
		await cosmosResult;
	}
}