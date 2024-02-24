using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.IntegrationTests.Fixtures;

namespace SegregatedStorage.IntegrationTests;

public abstract class BaseTests
{
	protected BaseTests(ContainerFixture fixture)
	{
		var services = new ServiceCollection();
		services.AddMongoFileRepository(fixture.MongoConnectionString, "test-db", "files");
		services.AddAzureStorageProvider();
		services.AddStorageService();
		Provider = services.BuildServiceProvider();
	}

	public ServiceProvider Provider { get; }
}