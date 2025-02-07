using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace SegregatedStorage.UnitTests;

public class SetupTests
{
	[Fact]
	public void AddStorageService_AlreadyRegistered_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddStorageService<int>();

		// Act && Assert
		Assert.Throws<InvalidOperationException>(() => services.AddStorageService<int>());
	}

	[Fact]
	public void AddStorageService_AlreadyRegisteredWithOtherKey_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddStorageService<int>();

		// Act
		var ex = Record.Exception(() => services.AddStorageService<Guid>());

		// Assert
		Assert.Null(ex);
	}

	[Fact]
	public void AddStorageService_NotRegistered_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddInMemoryFileRepository<int>();
		services.AddInMemoryStorageProvider<int>();

		// Act
		services.AddStorageService<int>();

		// Assert
		var provider = services.BuildServiceProvider();
		var service = provider.GetService<IStorageService<int>>();
		var deletionService = provider.GetServices<IHostedService>().OfType<DeletionBackgroundService<int>>().SingleOrDefault();

		Assert.NotNull(service);
		Assert.NotNull(deletionService);
		Assert.IsType<StorageService<int>>(service);
	}

	[Fact]
	public void AddStorageService_IncludeDeletionBackgroundServiceFalse_RegistersOnlyStorageService()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddInMemoryFileRepository<int>();
		services.AddInMemoryStorageProvider<int>();

		// Act
		services.AddStorageService<int>(config => config.IncludeDeletionBackgroundService = false);

		// Assert
		var provider = services.BuildServiceProvider();
		var service = provider.GetService<IStorageService<int>>();
		var deletionService = provider.GetServices<IHostedService>().OfType<DeletionBackgroundService<int>>().SingleOrDefault();

		Assert.NotNull(service);
		Assert.IsType<StorageService<int>>(service);
		Assert.Null(deletionService);
	}
	
	[Fact]
	public void AddInMemoryStorageProvider_NotRegistered_Registers()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddInMemoryStorageProvider<int>();

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IServiceLocator<int, IStorageProvider>>();

		Assert.NotNull(serviceLocator);
		var service = serviceLocator.GetService(42);

		Assert.IsType<InMemoryStorageProvider>(service);
	}

	[Fact]
	public void AddInMemoryStorageProvider_AlreadyRegistered_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddInMemoryStorageProvider<int>();

		// Act && Assert
		Assert.Throws<InvalidOperationException>(() => services.AddInMemoryStorageProvider<int>());
	}

	[Fact]
	public void AddInMemoryStorageProvider_AlreadyRegisteredWithOtherKey_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddInMemoryStorageProvider<int>();

		// Act
		var ex = Record.Exception(() => services.AddInMemoryStorageProvider<Guid>());

		// Assert
		Assert.Null(ex);
	}

	[Fact]
	public void AddInMemoryFileRepository_NotRegistered_Registers()
	{
		// Arrange
		var services = new ServiceCollection();

		// Act
		services.AddInMemoryFileRepository<int>();

		// Assert
		var provider = services.BuildServiceProvider();
		var serviceLocator = provider.GetService<IServiceLocator<int, IFileRepository>>();

		Assert.NotNull(serviceLocator);
		var service = serviceLocator.GetService(42);

		Assert.IsType<InMemoryFileRepository>(service);
	}

	[Fact]
	public void AddInMemoryFileRepository_AlreadyRegistered_Throws()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddInMemoryFileRepository<int>();

		// Act && Assert
		Assert.Throws<InvalidOperationException>(() => services.AddInMemoryFileRepository<int>());
	}

	[Fact]
	public void AddInMemoryFileRepository_AlreadyRegisteredWithOtherKey_Registers()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddInMemoryFileRepository<int>();

		// Act
		var ex = Record.Exception(() => services.AddInMemoryFileRepository<Guid>());

		// Assert
		Assert.Null(ex);
	}

	[Fact]
	public void MapStorageApi_NotMapped_DoesNotThrow()
	{
		// Arrange
		var builder = WebApplication.CreateBuilder();
		var app = builder.Build();

		// Act
		var ex = Record.Exception(() => app.MapStorageApi<int>());

		// Assert
		Assert.Null(ex);
	}
}