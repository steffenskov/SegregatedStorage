using DotNet.Testcontainers.Builders;
using Testcontainers.CosmosDb;

namespace SegregatedStorage.Cosmos.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly CosmosDbContainer _cosmosContainer;

	public ContainerFixture()
	{
		_cosmosContainer = new CosmosDbBuilder()
			.WithImage("mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:vnext-preview")
			.WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Emulator is accessible"))
			.Build();
	}

	public IServiceProvider Provider { get; private set; } = null!;

	public string ConnectionString => _cosmosContainer.GetConnectionString();

	public async ValueTask InitializeAsync()
	{
		await _cosmosContainer.StartAsync();
		var services = new ServiceCollection();
		services.AddCosmosFileRepository<int>(ConnectionString, "files", customerId => $"db-{customerId}");
		Provider = services.BuildServiceProvider();
	}

	public async ValueTask DisposeAsync()
	{
		await _cosmosContainer.DisposeAsync();
	}
}