using System.Security.Cryptography;
using System.Text;
using Common;
using Gdk;

namespace Client;

public static class PixbufLoader {
    private static readonly HttpClient _http = new();

    private static readonly CacheStore<string, Pixbuf?> _cacheStore = new(ResolveFromUrl);

    public static Task<Pixbuf?> GetFromUrl(string url) {
        return _cacheStore.GetAsync(url);
    }

    private static async Task<Pixbuf?> ResolveFromUrl(string url) {
        try {
            var str = await _http.GetStreamAsync(url);
            return new Pixbuf(str);
        }
        catch (Exception e) {
            Console.WriteLine("Failed downloading pixbuf");
            return null;
        }

        // TODO: Fix file locking when running multiple instances
        var hash = Base32Convert.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(url)));
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

        var file = File.Open(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return new Pixbuf(file);
    }
}