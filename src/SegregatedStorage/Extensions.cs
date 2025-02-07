using SegregatedStorage.Services;

namespace SegregatedStorage;

public static class Extensions
{
	private static bool HasService(this IServiceCollection services, Type type)
	{
		return services.Any(service => service.ServiceType == type);
	}

	private static bool HasService<T>(this IServiceCollection services)
	{
		return HasService(services, typeof(T));
	}

	public static void ThrowIfRegistered<T>(this IServiceCollection services)
	{
		if (services.HasService<T>())
			throw new InvalidOperationException($"An {typeof(T).Name} has already been injected into this IServiceCollection");
	}

	public static void ThrowIfKeyServiceLocatorRegistered<TKey, TService>(this IServiceCollection services)
	{
		if (services.HasService<IServiceLocator<TKey, TService>>())
			throw new InvalidOperationException($"An IKeyServiceLocator<{typeof(TKey).Name}, {typeof(TService).Name}> has already been injected into this IServiceCollection");
	}
}