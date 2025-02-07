using System.Collections.Concurrent;

namespace SegregatedStorage.Services;

internal class ServiceLocator<TKey, TService> : IServiceLocator<TKey, TService>
	where TKey : notnull
{
	private readonly Func<TKey, TService> _factoryMethod;
	private readonly object _lock = new();
	private readonly ConcurrentDictionary<TKey, TService> _services = new();

	public ServiceLocator(Func<TKey, TService> factoryMethod)
	{
		_factoryMethod = factoryMethod;
	}

	public TService GetService(TKey key)
	{
		if (_services.TryGetValue(key, out var service))
		{
			return service;
		}

		lock (_lock)
		{
			if (_services.TryGetValue(key, out service))
			{
				return service;
			}

			_services[key] = service = _factoryMethod(key);
			return service;
		}
	}

	public IEnumerable<(TKey Key, TService Service)> GetServices()
	{
		return _services.Select(pair => (pair.Key, pair.Value));
	}
}

public interface IServiceLocator<TKey, TService>
{
	TService GetService(TKey key);
	IEnumerable<(TKey Key, TService Service)> GetServices();
}