using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using SegregatedStorage.Repositories;

namespace SegregatedStorage;

public static class Setup
{
	/// <summary>
	///     Add MongoDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="connectionString">Connectionstring for MongoDB</param>
	/// <param name="collectionName">Name of the collection to store metadata in</param>
	/// <param name="databaseNameFactory">Factory method for creating database names for segregation</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
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

	/// <summary>
	///     Add MongoDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="connectionString">Connectionstring for MongoDB</param>
	/// <param name="collectionName">Name of the collection to store metadata in</param>
	/// <param name="databaseNameFactory">Factory method for creating database names for segregation</param>
	/// <param name="guidRepresentation">How guids are serialized to MongoDB</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddMongoFileRepository<TKey>(this IServiceCollection services, string connectionString, string collectionName,
		Func<TKey, string> databaseNameFactory, GuidRepresentation guidRepresentation)
		where TKey : notnull
	{
		RegisterGuidSerializer(guidRepresentation);
		return AddMongoFileRepository(services, connectionString, collectionName, databaseNameFactory);
	}

	/// <summary>
	///     Add MongoDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="connectionString">Connectionstring for MongoDB</param>
	/// <param name="collectionName">Name of the collection to store metadata in</param>
	/// <param name="databaseNameFactory">Factory method for creating database names for segregation</param>
	/// <param name="guidRepresentation">How guids are serialized to MongoDB</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddMongoFileRepository<TKey>(this IServiceCollection services, string connectionString, string collectionName,
		Func<TKey, string> databaseNameFactory, BsonType guidRepresentation)
		where TKey : notnull
	{
		RegisterGuidSerializer(guidRepresentation);
		return AddMongoFileRepository(services, connectionString, collectionName, databaseNameFactory);
	}

	/// <summary>
	///     Add MongoDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="collectionName">Name of the collection to store metadata in</param>
	/// <param name="databaseFactory">Factory method for creating a database instance for segregation</param>
	/// <param name="guidRepresentation">How guids are serialized to MongoDB</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
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

	/// <summary>
	///     Add MongoDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="collectionName">Name of the collection to store metadata in</param>
	/// <param name="databaseFactory">Factory method for creating a database instance for segregation</param>
	/// <param name="guidRepresentation">How guids are serialized to MongoDB</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddMongoFileRepository<TKey>(this IServiceCollection services, string collectionName,
		Func<TKey, IMongoDatabase> databaseFactory, GuidRepresentation guidRepresentation)
		where TKey : notnull
	{
		RegisterGuidSerializer(guidRepresentation);
		return AddMongoFileRepository(services, collectionName, databaseFactory);
	}

	/// <summary>
	///     Add MongoDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="collectionName">Name of the collection to store metadata in</param>
	/// <param name="databaseFactory">Factory method for creating a database instance for segregation</param>
	/// <param name="guidRepresentation">How guids are serialized to MongoDB</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddMongoFileRepository<TKey>(this IServiceCollection services, string collectionName,
		Func<TKey, IMongoDatabase> databaseFactory, BsonType guidRepresentation)
		where TKey : notnull
	{
		RegisterGuidSerializer(guidRepresentation);
		return AddMongoFileRepository(services, collectionName, databaseFactory);
	}

	private static void RegisterGuidSerializer(GuidRepresentation guidRepresentation)
	{
		try
		{
			BsonSerializer.RegisterSerializer(new GuidSerializer(guidRepresentation));
		}
		catch (BsonSerializationException)
		{
			// The above will throw if someone else already registered a Guid serializer for Mongo DB
		}
	}

	private static void RegisterGuidSerializer(BsonType bsonType)
	{
		try
		{
			BsonSerializer.RegisterSerializer(new GuidSerializer(bsonType));
		}
		catch (BsonSerializationException)
		{
			// The above will throw if someone else already registered a Guid serializer for Mongo DB
		}
	}
}