using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SegregatedStorage.Configuration;
using SegregatedStorage.Services;

namespace SegregatedStorage;

public static class Setup
{
	public static IServiceCollection AddStorageService(this IServiceCollection services)
	{
		if (services.Any(service => service.ServiceType == typeof(IStorageService)))
			throw new InvalidOperationException("AddStorageService has already been called once on this IServiceCollection");

		services.AddHostedService<DeletionBackgroundService>();
		return services.AddSingleton<IStorageService, StorageService>();
	}

	public static IServiceCollection AddInMemoryStorageProvider(this IServiceCollection services)
	{
		if (services.Any(service => service.ServiceType == typeof(IStorageProvider)))
			throw new InvalidOperationException("An IStorageProvider has already been injected into this IServiceCollection");

		return services.AddSingleton<IStorageProvider, InMemoryStorageProvider>();
	}

	public static IServiceCollection AddInMemoryFileRepository(this IServiceCollection services)
	{
		if (services.Any(service => service.ServiceType == typeof(IFileRepository)))
			throw new InvalidOperationException("An IFileRepository has already been injected into this IServiceCollection");

		return services.AddSingleton<IFileRepository, InMemoryFileRepository>();
	}

	public static void MapStorageApi(this IEndpointRouteBuilder app, Action<ApiConfiguration>? configureApi = null)
	{
		var configuration = new ApiConfiguration();
		configureApi?.Invoke(configuration);

		app.MapGet("/file/{id:guid}", async (IStorageService service, Guid id, CancellationToken cancellationToken) =>
			{
				try
				{
					var (file, data) = await service.DownloadAsync(id, cancellationToken);
					return Results.Stream(data, file.MimeType, file.FileName);
				}
				catch (FileNotFoundException)
				{
					return Results.NotFound();
				}
			})
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		var uploadMethod = app.MapPost("/file", async (IStorageService service, IFormFile file, CancellationToken cancellationToken) =>
		{
			await using var stream = file.OpenReadStream();
			var result = await service.UploadAsync(file.FileName, file.ContentType, stream, cancellationToken);

			return Results.Created((string?)null, new { Id = result });
		}).Produces(StatusCodes.Status201Created);

		if (configuration.DisableAntiForgery)
			uploadMethod.DisableAntiforgery();

		app.MapDelete("/file/{id:guid}", async (IStorageService service, Guid id, CancellationToken cancellationToken) =>
			{
				try
				{
					await service.DeleteAsync(id, cancellationToken);
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