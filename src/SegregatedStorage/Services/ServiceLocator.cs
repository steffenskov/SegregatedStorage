using System.Collections.Concurrent;

namespace SegregatedStorage.Services;

internal class AsyncServiceLocator<TKey, TService> : IAsyncServiceLocator<TKey, TService>
	where TKey : notnull
{
	private readonly Func<TKey, CancellationToken, ValueTask<TService>> _factoryMethod;
	private readonly SemaphoreSlim _lock = new(1, 1);
	private readonly ConcurrentDictionary<TKey, TService> _services = new();

	public AsyncServiceLocator(Func<TKey, CancellationToken, ValueTask<TService>> factoryMethod)
	{
		_factoryMethod = factoryMethod;
	}

	public AsyncServiceLocator(Func<TKey, TService> factoryMethod)
	{
		_factoryMethod = (key, _) => ValueTask.FromResult(factoryMethod(key));
	}

	public async ValueTask<TService> GetServiceAsync(TKey key, CancellationToken cancellationToken = default)
	{
		if (_services.TryGetValue(key, out var service))
		{
			return service;
		}

		await _lock.WaitAsync(cancellationToken);
		try
		{
			if (_services.TryGetValue(key, out service))
			{
				return service;
			}

			_services[key] = service = await _factoryMethod(key, cancellationToken);
			return service;
		}
		finally
		{
			_lock.Release();
		}
	}

	public IEnumerable<(TKey Key, TService Service)> GetServices()
	{
		return _services.Select(pair => (pair.Key, pair.Value));
	}
}

public interface IAsyncServiceLocator<TKey, TService>
{
	/// <summary>
	///     Lazily retrieves or creates a service for the given key.
	/// </summary>
	ValueTask<TService> GetServiceAsync(TKey key, CancellationToken cancellationToken = default);

	/// <summary>
	///     Retrieves all services that are currently created in the locator.
	/// </summary>
	IEnumerable<(TKey Key, TService Service)> GetServices();
}