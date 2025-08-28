using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace Touhou.Graphics;

public class Camera : GameSystem {


    public Vector2 Position { get; set; }

    public Vector2 View { get; set; }

    public float ViewAspectRatio { get => View.X / View.Y; }



    private NativeWindow window;


    public Camera(NativeWindow window) {
        this.window = window;
    }

    public Vector2 GetCameraSize(bool IsUI) {

        float aspectRatio = Game.Window.Size.X / (float)Game.Window.Size.Y;

        Vector2 size;

        if (IsUI) {
            size = new Vector2(2160f * aspectRatio, 2160f);
        } else {
            size = aspectRatio >= ViewAspectRatio ? new Vector2(View.Y * aspectRatio, View.Y) : new Vector2(View.X, View.X / aspectRatio);
        }

        return size;
    }

    public float GetCameraScale(bool isUI) {

        float aspectRatio = Game.Window.Size.X / (float)Game.Window.Size.Y;

        if (isUI) {
            return 2160f / window.ClientSize.Y;
        } else {
            return aspectRatio >= ViewAspectRatio ? View.Y / window.ClientSize.Y : View.X / window.ClientSize.X;
        }
    }


}