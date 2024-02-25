using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.Providers;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddAzureStorageProvider<TKey>(this IServiceCollection services, string connectionString,
		Func<TKey, string> blobContainerNameFactory)
		where TKey : notnull
	{
		var blobServiceClient = new BlobServiceClient(connectionString);

		return services.AddKeyServiceLocator<TKey, IStorageProvider>(key => new AzureStorageProvider(blobServiceClient, blobContainerNameFactory(key)));
	}
}