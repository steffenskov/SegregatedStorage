using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.Aggregates;
using SegregatedStorage.IntegrationTests.Fixtures;
using SegregatedStorage.Repositories;

namespace SegregatedStorage.IntegrationTests;

[Collection(nameof(ConfigurationCollection))]
public class MongoFileRepositoryTests : BaseTests
{
	private readonly IFileRepository _repository;

	public MongoFileRepositoryTests(ContainerFixture fixture) : base(fixture)
	{
		_repository = Provider.GetRequiredService<IFileRepository>();
	}

	[Fact]
	public async Task PersistAsync_Valid_IsPersisted()
	{
		// Arrange
		var file = FileAggregate.Create("image.jpg", "image/jpg");

		// Act
		await _repository.PersistAsync(file);

		// Assert
		var fetched = await _repository.GetAsync(file.Id);
		Assert.Equal(file, fetched);
	}
}