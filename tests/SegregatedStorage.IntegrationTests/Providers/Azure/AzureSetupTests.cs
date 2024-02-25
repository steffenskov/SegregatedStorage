namespace SegregatedStorage.IntegrationTests.Providers.Azure;

[Collection(nameof(ConfigurationCollection))]
public class AzureSetupTests : BaseTests
{
	private readonly string _connectionString;

	public AzureSetupTests(ContainerFixture fixture) : base(fixture)
	{
		_connectionString = fixture.AzureConnectionString;
	}

	[Fact]
	public void AddAzureStorageProvider_DoesNotExist_IsAdded()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddAzureStorageProvider<int>(_connectionString, key => $"container-{key}");

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IKeyServiceLocator<int, IStorageProvider>>();

		Assert.NotNull(serviceLocator);
		var service = serviceLocator.GetService(42);
		Assert.IsType<AzureStorageProvider>(service);
	}
}