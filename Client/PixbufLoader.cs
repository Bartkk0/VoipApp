using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;
using Common;
using Gdk;

namespace Client;

public static class PixbufLoader {
    private static HttpClient _http = new();
    private static Dictionary<string, SemaphoreSlim> _queue = new();
    private static Dictionary<string, Pixbuf> _cache = new();

    private static CacheStore<string, Pixbuf> _cacheStore = new(ResolveFromUrl);

    public static Task<Pixbuf> GetFromUrl(string url) {
        return _cacheStore.GetAsync(url);
    }

    private static async Task<Pixbuf> ResolveFromUrl(string url) {
        string hash = Base32Convert.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(url)));
        var imagePath = Path.Combine(new[] { "image_cache", hash + Path.GetExtension(url) });

        if (!File.Exists(imagePath)) {
            Directory.CreateDirectory("image_cache");

            var image = File.Create(imagePath);

            var stream = await _http.GetStreamAsync(url);
            await stream.CopyToAsync(image);
            // var pixbuf = new Pixbuf(stream);
            image.Close();
            // return pixbuf;
        }

        var file = File.OpenRead(imagePath);
        return new Pixbuf(file);
    }
}