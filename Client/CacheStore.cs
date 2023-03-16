namespace Client;

public class CacheStore<TKey, TValue> where TKey : notnull {
    private readonly Dictionary<TKey, TValue> _cache = new();
    private readonly Func<TKey, Task<TValue>> _resolver;
    private readonly Dictionary<TKey, Task<TValue>> _resolvers = new();

    public CacheStore(Func<TKey, Task<TValue>> resolver) {
        _resolver = resolver;
    }

    public async Task<TValue> GetAsync(TKey key) {
        // Console.WriteLine("CacheStore: GetAsync");
        if (_cache.ContainsKey(key))
            // Console.WriteLine("CacheStore: Cache hit");
            return _cache[key];

        if (_resolvers.ContainsKey(key)) {
            // Console.WriteLine("CacheStore: Already resolving");
            await _resolvers[key];
            return _cache[key];
        }

        var res = _resolver.Invoke(key);
        _resolvers.Add(key, res);
        var result = await res;
        _cache.Add(key, result);

        // Console.WriteLine("CacheStore: Resolved and cached");
        return result;
    }
}