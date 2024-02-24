using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SegregatedStorage.Repositories;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddMongoFileRepository(this IServiceCollection services, string connectionString, string databaseName, string collectionName)
	{
		if (services.Any(service => service.ServiceType == typeof(IFileRepository)))
			throw new InvalidOperationException("An IFileRepository has already been injected into this IServiceCollection");

		var mongoClient = new MongoClient(connectionString);
		var db = mongoClient.GetDatabase(databaseName);

		return services.AddSingleton<IFileRepository>(new MongoFileRepository(db, collectionName));
	}
}