using OpenTK.Mathematics;

namespace Touhou;

public class OldCamera {


    public OldCamera(Vector2 startingFocusedSize) {
        WorldFocusedSize = startingFocusedSize;
    }

    public Vector2 Position { get; set; }
    public Vector2 WorldFocusedSize { get; set; } // the area in world coords that always fits inside the window's view port

    public float UHDWidth { get; } = 3840f;
    public float UHDHeight { get; } = 2160f;

    public float WorldToScreenScale => UHDHeight / WorldFocusedSize.Y;

    public Vector2 FullSize {
        get {
            var windowAspectRatio = Game.AspectRatio;
            bool largerRatio = windowAspectRatio >= FocusedAspectRatio;

            return new Vector2() {
                X = largerRatio ? WorldFocusedSize.Y * windowAspectRatio : WorldFocusedSize.X,
                Y = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / windowAspectRatio
            };

        }
    }

    public Vector2 UHDFullSize => UHDHeight / WorldFocusedSize.Y * FullSize;

    public float FocusedAspectRatio { get => WorldFocusedSize.X / WorldFocusedSize.Y; }


    public float WorldToCurrentScreenScale {
        get {
            bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
            float worldHeight = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;
            return Game.WindowSize.Y / worldHeight;
        }
    }

    public float WorldToUHDScreenScale {
        get {
            bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
            float worldHeight = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;
            float uHDHeight = largerRatio ? 2160f : 3840f / Game.AspectRatio;
            return uHDHeight / worldHeight;
        }
    }

    public float GetScreenScale(bool isUI) {
        bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
        float worldHeight = isUI ? (largerRatio ? UHDHeight : UHDWidth / Game.AspectRatio) : (largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio);
        return Game.WindowSize.Y / worldHeight;
    }

    public Vector2 GetScreenCoords(Vector2 coords, bool isUI) {
        if (isUI) {
            return Game.WindowSize * coords;
        } else {
            bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
            float width = largerRatio ? WorldFocusedSize.Y * Game.AspectRatio : WorldFocusedSize.X;
            float height = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;

            coords -= Position;
            return new Vector2() {
                X = (coords.X / width + 0.5f) * Game.WindowSize.X,
                Y = (coords.Y / height + 0.5f) * Game.WindowSize.Y
            };
        }
    }

    public Vector2 WorldToScreenCoords(Vector2 worldCoords) {
        bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
        float width = largerRatio ? WorldFocusedSize.Y * Game.AspectRatio : WorldFocusedSize.X;
        float height = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;

        worldCoords -= Position;
        return new Vector2() {
            X = (worldCoords.X / width + 0.5f) * Game.WindowSize.X,
            Y = (worldCoords.Y / height + 0.5f) * Game.WindowSize.Y
        };

    }

    public Vector2 WorldToFullScreenCoords(Vector2 worldCoords) {
        var fullSize = FullSize;
        System.Console.WriteLine(fullSize);

        var uHDFullSize = UHDFullSize;


        return new Vector2() {
            X = ((worldCoords.X - Position.X) / fullSize.X + 0.5f) * uHDFullSize.X,
            Y = ((worldCoords.Y - Position.Y) / fullSize.Y + 0.5f) * uHDFullSize.Y
        };
    }

    public Vector2 PercentageToFullScreenCoords(Vector2 percentage) {

        var uHDFullSize = UHDFullSize;

        return new Vector2() {
            X = percentage.X * uHDFullSize.X,
            Y = percentage.Y * uHDFullSize.Y
        };
    }

    public Vector2 WorldToFocusedScreenCoords(Vector2 worldCoords) {
        return new Vector2() {
            X = (worldCoords.X - Position.X) / WorldFocusedSize.X + 0.5f,
            Y = (worldCoords.Y - Position.Y) / WorldFocusedSize.Y + 0.5f
        };
    }
}