using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.Repositories;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddMongoFileRepository(this IServiceCollection services)
	{
		if (services.Any(service => service.ServiceType == typeof(IFileRepository)))
			throw new InvalidOperationException("An IFileRepository has already been injected into this IServiceCollection");

		return services.AddSingleton<IFileRepository, MongoFileRepository>();
	}
}