using System.Security.Cryptography;
using Common;

namespace Server;

public class ImageStore {
    public ImageStore() {
        Directory.CreateDirectory("blobs");
    }

    public byte[] GetByHash(string hash) {
        return File.ReadAllBytes(Path.Combine("blobs", hash));
    }

    public string Store(byte[] data) {
        var hash = SHA1.HashData(data);
        var base32 = Base32Convert.Encode(hash);
        var file = File.OpenWrite(Path.Combine("blobs", base32));
        file.Write(data);
        file.Close();
        return base32;
    }
}