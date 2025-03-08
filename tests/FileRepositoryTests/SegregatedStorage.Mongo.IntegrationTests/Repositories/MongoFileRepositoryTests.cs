namespace SegregatedStorage.Mongo.IntegrationTests.Repositories;

[Collection(nameof(ConfigurationCollection))]
public class MongoFileRepositoryTests : FileRepositoryTests
{
	public MongoFileRepositoryTests(ContainerFixture fixture) : base(fixture.Provider.GetRequiredService<IAsyncServiceLocator<int, IFileRepository>>())
	{
	}
}