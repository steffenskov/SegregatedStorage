using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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
			services.AddHostedService<DeletionBackgroundService<TKey>>();
		return services.AddSingleton<IStorageService<TKey>, StorageService<TKey>>();
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

	public static IServiceCollection AddKeyServiceLocator<TKey, TService>(this IServiceCollection services, Func<TKey, TService> factoryMethod)
		where TKey : notnull
	{
		services.ThrowIfKeyServiceLocatorRegistered<TKey, TService>();

		return services.AddSingleton<IKeyServiceLocator<TKey, TService>>(new KeyServiceLocator<TKey, TService>(factoryMethod));
	}

	public static void MapStorageApi<TKey>(this IEndpointRouteBuilder app, Action<ApiConfiguration>? configureApi = null)
		where TKey : notnull
	{
		var configuration = new ApiConfiguration();
		configureApi?.Invoke(configuration);

		app.MapGet($"/{configuration.EndpointPrefix}/{{key}}/{{id:guid}}", async (IStorageService<TKey> service, TKey key, Guid id, CancellationToken cancellationToken) =>
			{
				try
				{
					var (file, data) = await service.DownloadAsync(key, id, cancellationToken);
					return Results.Stream(data, file.MimeType, file.FileName);
				}
				catch (FileNotFoundException)
				{
					return Results.NotFound();
				}
			})
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		var uploadMethod = app.MapPost($"/{configuration.EndpointPrefix}/{{key}}",
			async (IStorageService<TKey> service, TKey key, IFormFile file, CancellationToken cancellationToken) =>
			{
				await using var stream = file.OpenReadStream();
				var result = await service.UploadAsync(key, file.FileName, file.ContentType, stream, cancellationToken);

				return Results.Created((string?)null, new { Id = result });
			}).Produces(StatusCodes.Status201Created);

		if (configuration.DisableAntiForgery)
			uploadMethod.DisableAntiforgery();

		app.MapDelete($"/{configuration.EndpointPrefix}/{{key}}/{{id:guid}}", async (IStorageService<TKey> service, TKey key, Guid id, CancellationToken cancellationToken) =>
			{
				try
				{
					await service.DeleteAsync(key, id, cancellationToken);
				}
				catch (FileNotFoundException)
				{
					return Results.NotFound();
				}

				return Results.NoContent();
			})
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);
	}
}