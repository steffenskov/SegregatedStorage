using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.Repositories;

namespace SegregatedStorage;

public static class Setup
{
	/// <summary>
	///     Add CosmosDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="connectionString">Connectionstring for CosmosDB</param>
	/// <param name="containerName">Name of the container to store metadata in</param>
	/// <param name="databaseNameFactory">Factory method for creating database names for segregation</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddCosmosFileRepository<TKey>(this IServiceCollection services, string connectionString, string containerName,
		Func<TKey, string> databaseNameFactory)
		where TKey : notnull
	{
		var client = CreateClient(connectionString);

		return services.AddKeyServiceLocator<TKey, IFileRepository>(async (key, cancellationToken) =>
		{
			var dbName = databaseNameFactory(key);
			var db = await CreateDatabaseAsync(client, dbName, cancellationToken);
			var container = await CreateContainerAsync(db, containerName, cancellationToken);

			return new CosmosFileRepository(container);
		});
	}

	/// <summary>
	///     Add CosmosDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="containerName">Name of the container to store metadata in</param>
	/// <param name="databaseFactory">Factory method for creating a database for segregation</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddCosmosFileRepository<TKey>(this IServiceCollection services, string containerName,
		Func<TKey, CancellationToken, ValueTask<Database>> databaseFactory)
		where TKey : notnull
	{
		return services.AddKeyServiceLocator<TKey, IFileRepository>(async (key, cancellationToken) =>
		{
			var db = await databaseFactory(key, cancellationToken);
			var container = await CreateContainerAsync(db, containerName, cancellationToken);

			return new CosmosFileRepository(container);
		});
	}

	/// <summary>
	///     Add CosmosDB based File Repository for storing metadata.
	/// </summary>
	/// <param name="containerFactory">Factory method for creating a container for segregation</param>
	/// <typeparam name="TKey">Type of segregation key</typeparam>
	public static IServiceCollection AddCosmosFileRepository<TKey>(this IServiceCollection services, Func<TKey, CancellationToken, ValueTask<Container>> containerFactory)
		where TKey : notnull
	{
		return services.AddKeyServiceLocator<TKey, IFileRepository>(async (key, cancellationToken) =>
		{
			var container = await containerFactory(key, cancellationToken);

			return new CosmosFileRepository(container);
		});
	}

	private static async Task<Database> CreateDatabaseAsync(CosmosClient client, string dbName, CancellationToken cancellationToken)
	{
		var dbResponse = await client.CreateDatabaseIfNotExistsAsync(dbName, cancellationToken: cancellationToken);
		if (dbResponse.StatusCode != HttpStatusCode.OK && dbResponse.StatusCode != HttpStatusCode.Created)
		{
			throw new InvalidOperationException($"Failed to create database {dbName}");
		}

		var db = dbResponse.Database;
		return db;
	}

	private static async Task<Container> CreateContainerAsync(Database db, string containerName, CancellationToken cancellationToken)
	{
		var containerResponse = await db.CreateContainerIfNotExistsAsync(new ContainerProperties(containerName, Consts.PartitionKey), cancellationToken: cancellationToken);
		if (containerResponse.StatusCode != HttpStatusCode.OK && containerResponse.StatusCode != HttpStatusCode.Created)
		{
			throw new InvalidOperationException($"Failed to create container {containerName}");
		}

		var container = containerResponse.Container;
		return container;
	}

	private static CosmosClient CreateClient(string connectionString)
	{
		if (connectionString.Contains(Consts.EmulatorDefaultAccountKey))
		{
			return new CosmosClient(connectionString.Replace("https", "http"), new CosmosClientOptions
			{
				HttpClientFactory = () =>
				{
					HttpMessageHandler httpMessageHandler = new HttpClientHandler
					{
						ServerCertificateCustomValidationCallback = (_, _, _, _) => true
					};
					return new HttpClient(httpMessageHandler);
				},
				ConnectionMode = ConnectionMode.Gateway,
				LimitToEndpoint = true
			});
		}

		return new CosmosClient(connectionString);
	}
}