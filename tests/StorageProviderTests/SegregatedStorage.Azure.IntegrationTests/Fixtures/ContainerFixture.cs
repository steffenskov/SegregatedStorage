using Testcontainers.Azurite;

namespace SegregatedStorage.Azure.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly AzuriteContainer _azureContainer;

	public ContainerFixture()
	{
		_azureContainer = new AzuriteBuilder()
			.WithImage("mcr.microsoft.com/azure-storage/azurite:latest")
			.WithCommand("--skipApiVersionCheck")
			.Build();
	}

	public string ConnectionString => _azureContainer.GetConnectionString();

	public IServiceProvider Provider { get; private set; } = null!;

	public async ValueTask InitializeAsync()
	{
		await _azureContainer.StartAsync();

		var services = new ServiceCollection();
		services.AddAzureStorageProvider<int>(ConnectionString, customerId => $"container-{customerId}");
		services.AddStorageService<int>();
		Provider = services.BuildServiceProvider();
	}

	public async ValueTask DisposeAsync()
	{
		await _azureContainer.DisposeAsync();
	}
}