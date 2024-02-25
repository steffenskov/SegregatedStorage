namespace SegregatedStorage.IntegrationTests;

public abstract class BaseTests
{
	protected BaseTests(ContainerFixture fixture)
	{
		var services = new ServiceCollection();
		services.AddMongoFileRepository<int>(fixture.MongoConnectionString, "files", customerId => $"db-{customerId}");
		services.AddAzureStorageProvider<int>(fixture.AzureConnectionString, customerId => $"container-{customerId}");
		services.AddStorageService<int>();
		Provider = services.BuildServiceProvider();
	}

	protected ServiceProvider Provider { get; }
}