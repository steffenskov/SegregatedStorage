using Testcontainers.Azurite;
using Testcontainers.MongoDb;

namespace SegregatedStorage.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly AzuriteContainer _azureContainer;
	private readonly MongoDbContainer _mongoContainer;

	public ContainerFixture()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithUsername("mongo")
			.WithPassword("secret")
			.Build();

		_azureContainer = new AzuriteBuilder()
			.WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
			.WithCommand("--skipApiVersionCheck")
			.Build();
	}

	public string MongoConnectionString => _mongoContainer.GetConnectionString();
	public string AzureConnectionString => _azureContainer.GetConnectionString();

	public async Task InitializeAsync()
	{
		await Task.WhenAll(_mongoContainer.StartAsync(), _azureContainer.StartAsync());
	}

	public async Task DisposeAsync()
	{
		await _mongoContainer.DisposeAsync();
		await _azureContainer.DisposeAsync();
	}
}