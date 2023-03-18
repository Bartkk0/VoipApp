using Gdk;

namespace Client.Types;

public record User {
    public string _imageHash;
    public ChatClient Client;

    public bool Online;
    public uint UserId;
    public string Username;

    public string ImageHash {
        get => _imageHash;
        set {
            if (_imageHash != value) {
                _imageHash = value;
                OnProfileImageChanged?.Invoke();
            }
        }
    }

    public event Action? OnProfileImageChanged;

    public async Task<byte[]?> GetImageBytes() {
        if (string.IsNullOrEmpty(_imageHash))
            return null;
        
        return await Client.ProfileStore.GetAsync(ImageHash);
    }

    public async Task<Pixbuf?> GetResizedImage(int width, int height) {
        var bytes = await GetImageBytes();
        if (bytes == null) return null;

        var pixbuf = new Pixbuf(bytes);
        var resized = pixbuf.ScaleSimple(width, height, InterpType.Bilinear);

        return resized;
    }
}