namespace SegregatedStorage.UnitTests.Services;

public class KeyServiceLocatorTests
{
	[Fact]
	public void GetService_DoesNotExist_IsCreatedByFactory()
	{
		// Arrange
		var created = false;
		var locator = new KeyServiceLocator<int, string>(val =>
		{
			created = true;
			return val.ToString();
		});

		// Act
		var service = locator.GetService(42);

		// Assert
		Assert.True(created);
	}

	[Fact]
	public void GetService_Exists_ReturnsSameInstance()
	{
		// Arrange
		var locator = new KeyServiceLocator<int, Service>(val => new Service(val));

		// Act
		var firstService = locator.GetService(42);
		var secondService = locator.GetService(42);

		// Assert
		Assert.Same(firstService, secondService);
	}

	[Fact]
	public void GetServices_NoneCreated_ReturnsEmptyCollection()
	{
		// Arrange
		var locator = new KeyServiceLocator<int, Service>(val => new Service(val));

		// Act
		var services = locator.GetServices();

		// Assert
		Assert.Empty(services);
	}

	[Fact]
	public void GetServices_SomeCreated_ReturnsThose()
	{
		// Arrange
		var locator = new KeyServiceLocator<int, Service>(val => new Service(val));
		var service1 = locator.GetService(42);
		var service2 = locator.GetService(1337);

		// Act
		var services = locator.GetServices().ToList();

		// Assert
		Assert.Contains((42, service1), services);
		Assert.Contains((1337, service2), services);
	}
}

file class Service(int Value);