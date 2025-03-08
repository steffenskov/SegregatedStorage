using MongoDB.Bson;
using Testcontainers.MongoDb;

namespace SegregatedStorage.Mongo.IntegrationTests.Fixtures;

public class ContainerFixture : IAsyncLifetime
{
	private readonly MongoDbContainer _mongoContainer;

	public ContainerFixture()
	{
		_mongoContainer = new MongoDbBuilder()
			.WithImage("mongo:latest")
			.WithUsername("mongo")
			.WithPassword("secret")
			.Build();
	}

	public IServiceProvider Provider { get; private set; } = null!;

	public string ConnectionString => _mongoContainer.GetConnectionString();

	public async Task InitializeAsync()
	{
		await _mongoContainer.StartAsync();

		var services = new ServiceCollection();
		services.AddMongoFileRepository<int>(ConnectionString, "files", customerId => $"db-{customerId}", GuidRepresentation.Standard);
		Provider = services.BuildServiceProvider();
	}

	public async Task DisposeAsync()
	{
		await _mongoContainer.DisposeAsync();
	}
}