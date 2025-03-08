using SegregatedStorage.StorageProvider.IntegrationTests.StorageProviders;

namespace SegregatedStorage.Azure.IntegrationTests.Providers.Azure;

[Collection(nameof(ConfigurationCollection))]
public class AzureStorageProviderTests : StorageProviderTests
{
	public AzureStorageProviderTests(ContainerFixture fixture) : base(fixture.Provider.GetRequiredService<IAsyncServiceLocator<int, IStorageProvider>>())
	{
	}
}