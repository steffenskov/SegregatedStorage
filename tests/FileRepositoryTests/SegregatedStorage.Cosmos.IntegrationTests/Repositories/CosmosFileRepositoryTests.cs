namespace SegregatedStorage.Cosmos.IntegrationTests.Repositories;

[Collection(nameof(ConfigurationCollection))]
public class CosmosFileRepositoryTests : FileRepositoryTests
{
	public CosmosFileRepositoryTests(ContainerFixture fixture) : base(fixture.Provider.GetRequiredService<IAsyncServiceLocator<int, IFileRepository>>())
	{
	}
}