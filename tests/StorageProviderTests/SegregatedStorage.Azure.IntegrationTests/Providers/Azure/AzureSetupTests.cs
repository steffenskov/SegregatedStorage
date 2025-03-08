namespace SegregatedStorage.Azure.IntegrationTests.Providers.Azure;

[Collection(nameof(ConfigurationCollection))]
public class AzureSetupTests
{
	private readonly string _connectionString;

	public AzureSetupTests(ContainerFixture fixture)
	{
		_connectionString = fixture.ConnectionString;
	}

	[Fact]
	public async Task AddAzureStorageProvider_DoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddAzureStorageProvider<int>(_connectionString, key => $"container-{key}");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IAsyncServiceLocator<int, IStorageProvider>>();

		Assert.NotNull(serviceLocator);
		var service = await serviceLocator.GetServiceAsync(42);
		Assert.IsType<AzureStorageProvider>(service);
	}
}