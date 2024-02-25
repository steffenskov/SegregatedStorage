using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.IntegrationTests.Fixtures;

namespace SegregatedStorage.IntegrationTests;

public abstract class BaseTests
{
	protected BaseTests(ContainerFixture fixture)
	{
		var services = new ServiceCollection();
		services.AddMongoFileRepository<int>(fixture.MongoConnectionString, "test-db", customerId => $"files-{customerId}");
		//services.AddAzureStorageProvider<int>();
		services.AddStorageService<int>();
		Provider = services.BuildServiceProvider();
	}

	public ServiceProvider Provider { get; }
}