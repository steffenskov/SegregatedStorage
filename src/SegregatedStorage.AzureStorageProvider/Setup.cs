using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.Providers;
using SegregatedStorage.Repositories;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddAzureStorageProvider(this IServiceCollection services)
	{
		if (services.Any(service => service.ServiceType == typeof(IStorageProvider)))
			throw new InvalidOperationException("An IStorageProvider has already been injected into this IServiceCollection");

		return services.AddSingleton<IStorageProvider, AzureStorageProvider>();
	}
}