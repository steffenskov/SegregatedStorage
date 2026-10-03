using SegregatedStorage.Configuration;
using SegregatedStorage.Services;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddStorageService<TKey>(this IServiceCollection services, Action<StorageServiceConfiguration>? configureService = null)
		where TKey : notnull
	{
		services.ThrowIfRegistered<IStorageService<TKey>>();

		var configuration = new StorageServiceConfiguration();
		configureService?.Invoke(configuration);

		if (configuration.IncludeDeletionBackgroundService)
		{
			services.AddHostedService<DeletionBackgroundService<TKey>>();
		}

		return services.AddSingleton<IStorageService<TKey>>(provider =>
			new StorageService<TKey>(
				provider.GetRequiredService<IAsyncServiceLocator<TKey, IFileRepository>>(),
				provider.GetRequiredService<IAsyncServiceLocator<TKey, IStorageProvider>>(),
				configuration.HashAlgorithm)
		);
	}

	public static IServiceCollection AddInMemoryStorageProvider<TKey>(this IServiceCollection services)
		where TKey : notnull
	{
		return services.AddKeyServiceLocator<TKey, IStorageProvider>(_ => new InMemoryStorageProvider());
	}

	public static IServiceCollection AddInMemoryFileRepository<TKey>(this IServiceCollection services)
		where TKey : notnull
	{
		return services.AddKeyServiceLocator<TKey, IFileRepository>(_ => new InMemoryFileRepository());
	}

	public static IServiceCollection AddKeyServiceLocator<TKey, TService>(this IServiceCollection services, Func<TKey, CancellationToken, ValueTask<TService>> factoryMethod)
		where TKey : notnull
	{
		services.ThrowIfKeyServiceLocatorRegistered<TKey, TService>();

		return services.AddSingleton<IAsyncServiceLocator<TKey, TService>>(new AsyncServiceLocator<TKey, TService>(factoryMethod));
	}

	public static IServiceCollection AddKeyServiceLocator<TKey, TService>(this IServiceCollection services, Func<TKey, TService> factoryMethod)
		where TKey : notnull
	{
		services.ThrowIfKeyServiceLocatorRegistered<TKey, TService>();

		return services.AddSingleton<IAsyncServiceLocator<TKey, TService>>(new AsyncServiceLocator<TKey, TService>(factoryMethod));
	}
}