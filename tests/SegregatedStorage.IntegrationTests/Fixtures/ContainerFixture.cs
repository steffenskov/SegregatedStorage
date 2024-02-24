using Testcontainers.MongoDb;

namespace SegregatedStorage.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly MongoDbContainer _mongoContainer;

	public ContainerFixture()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithUsername("mongo")
			.WithPassword("secret")
			.Build();
	}

	public string MongoConnectionString => _mongoContainer.GetConnectionString();

	public async Task InitializeAsync()
	{
		await _mongoContainer.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await _mongoContainer.DisposeAsync();
	}
}