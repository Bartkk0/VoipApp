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

    public async Task<byte[]> GetImageBytes() {
        return await Client.ProfileStore.GetAsync(ImageHash);
    }
}