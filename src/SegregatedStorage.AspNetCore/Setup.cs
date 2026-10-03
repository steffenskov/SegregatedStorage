using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SegregatedStorage.AspNetCore.Configuration;
using SegregatedStorage.Services;

namespace SegregatedStorage;

public static class Setup
{
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
		{
			uploadMethod.DisableAntiforgery();
		}

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