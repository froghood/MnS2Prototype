
namespace OsuEditor;

public struct BindlessTextureData {
    public long Handle { get; }

    public BindlessTextureData(long handle) {
        Handle = handle;
    }

    public override string ToString() {
        return $"Handle: {Handle}";
    }
}