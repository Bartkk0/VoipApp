using Gdk;

namespace Client;

public static class PixbufLoader
{
    private static HttpClient _http = new();
    private static Dictionary<string, SemaphoreSlim> _queue = new();
    private static Dictionary<string, Pixbuf> _cache = new();

    private static CacheStore<string, Pixbuf> _cacheStore = new(ResolveFromUrl);

    public static Task<Pixbuf> GetFromUrl(string url)
    {
        return _cacheStore.GetAsync(url);
    }

    private static async Task<Pixbuf> ResolveFromUrl(string url)
    {
        var stream = await _http.GetStreamAsync(url);
        var pixbuf = new Pixbuf(stream);

        return pixbuf;
    }
}