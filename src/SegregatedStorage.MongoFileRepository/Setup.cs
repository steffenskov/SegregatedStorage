using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using SegregatedStorage.Repositories;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddMongoFileRepository<TKey>(this IServiceCollection services, string connectionString, string collectionName,
		Func<TKey, string> databaseNameFactory)
		where TKey : notnull
	{
		var mongoClient = new MongoClient(connectionString);

		return services.AddKeyServiceLocator<TKey, IFileRepository>(key =>
		{
			var dbName = databaseNameFactory(key);
			var db = mongoClient.GetDatabase(dbName);

			return new MongoFileRepository(db, collectionName);
		});
	}

	public static IServiceCollection AddMongoFileRepository<TKey>(this IServiceCollection services, string collectionName,
		Func<TKey, IMongoDatabase> databaseFactory)
		where TKey : notnull
	{
		return services.AddKeyServiceLocator<TKey, IFileRepository>(key =>
		{
			var db = databaseFactory(key);

			return new MongoFileRepository(db, collectionName);
		});
	}
}