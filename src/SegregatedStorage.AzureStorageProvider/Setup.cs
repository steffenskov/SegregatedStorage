using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using SegregatedStorage.Providers;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddAzureStorageProvider<TKey>(this IServiceCollection services, BlobServiceClient blobServiceClient,
		Func<TKey, string> blobContainerNameFactory)
		where TKey : notnull
	{
		return services.AddKeyServiceLocator<TKey, IStorageProvider>(key => new AzureStorageProvider(blobServiceClient, blobContainerNameFactory(key)));
	}
}