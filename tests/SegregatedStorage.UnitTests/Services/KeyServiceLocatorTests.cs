namespace SegregatedStorage.UnitTests.Services;

public class AsyncServiceLocatorTests
{
	[Fact]
	public async Task GetService_DoesNotExist_IsCreatedByFactory()
	{
		// Arrange
		var created = false;
		var locator = new AsyncServiceLocator<int, string>((val, _) =>
		{
			created = true;
			return ValueTask.FromResult(val.ToString());
		});

		// Act
		await locator.GetServiceAsync(42, TestContext.Current.CancellationToken);

		// Assert
		Assert.True(created);
	}

	[Fact]
	public async Task GetService_Exists_ReturnsSameInstance()
	{
		// Arrange
		var locator = new AsyncServiceLocator<int, Service>((val, _) => ValueTask.FromResult(new Service()));

		// Act
		var firstService = await locator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var secondService = await locator.GetServiceAsync(42, TestContext.Current.CancellationToken);

		// Assert
		Assert.Same(firstService, secondService);
	}

	[Fact]
	public void GetServices_NoneCreated_ReturnsEmptyCollection()
	{
		// Arrange
		var locator = new AsyncServiceLocator<int, Service>((val, _) => ValueTask.FromResult(new Service()));

		// Act
		var services = locator.GetServices();

		// Assert
		Assert.Empty(services);
	}

	[Fact]
	public async Task GetServices_SomeCreated_ReturnsThose()
	{
		// Arrange
		var locator = new AsyncServiceLocator<int, Service>((val, _) => ValueTask.FromResult(new Service()));
		var service1 = await locator.GetServiceAsync(42, TestContext.Current.CancellationToken);
		var service2 = await locator.GetServiceAsync(1337, TestContext.Current.CancellationToken);

		// Act
		var services = locator.GetServices().ToList();

		// Assert
		Assert.Contains((42, service1), services);
		Assert.Contains((1337, service2), services);
	}
}

file class Service;