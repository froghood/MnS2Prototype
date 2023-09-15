using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Touhou.Graphics;

public class Camera {


    public Vector2 Position { get; set; }

    public Vector2 View { get; set; }

    public float ViewAspectRatio { get => View.X / View.Y; }



    private NativeWindow window;


    public Camera(NativeWindow window) {
        this.window = window;
    }

    public Vector2 GetCameraSize(bool IsUI) {
        Vector2 size;

        if (IsUI) {
            size = new Vector2(2160f * Game.AspectRatio, 2160f);
        } else {
            size = Game.AspectRatio >= ViewAspectRatio ? new Vector2(View.Y * Game.AspectRatio, View.Y) : new Vector2(View.X, View.X / Game.AspectRatio);
        }

        return size;
    }

    public float GetCameraScale(bool isUI) {
        if (isUI) {
            return 2160f / window.ClientSize.Y;
        } else {
            return Game.AspectRatio >= ViewAspectRatio ? View.Y / window.ClientSize.Y : View.X / window.ClientSize.X;
        }
    }


}