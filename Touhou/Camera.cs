using SFML.Graphics;
using SFML.System;

namespace Touhou;

public class Camera {


    public Camera(Vector2f startingFocusedSize) {
        WorldFocusedSize = startingFocusedSize;
    }

    public Vector2f Position { get; set; }
    public Vector2f WorldFocusedSize { get; set; } // the area in world coords that always fits inside the window's view port

    public float UHDWidth { get; } = 3840f;
    public float UHDHeight { get; } = 2160f;

    public float WorldToScreenScale => UHDHeight / WorldFocusedSize.Y;

    public Vector2f FullSize {
        get {
            var windowAspectRatio = Game.AspectRatio;
            bool largerRatio = windowAspectRatio >= FocusedAspectRatio;

            return new Vector2f() {
                X = largerRatio ? WorldFocusedSize.Y * windowAspectRatio : WorldFocusedSize.X,
                Y = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / windowAspectRatio
            };

        }
    }

    public Vector2f UHDFullSize => UHDHeight / WorldFocusedSize.Y * FullSize;

    public float FocusedAspectRatio { get => WorldFocusedSize.X / WorldFocusedSize.Y; }


    public float WorldToCurrentScreenScale {
        get {
            bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
            float worldHeight = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;
            return Game.Window.Size.Y / worldHeight;
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
        return Game.Window.Size.Y / worldHeight;
    }

    public Vector2f GetScreenCoords(Vector2f coords, bool isUI) {
        if (isUI) {
            return new Vector2f() {
                X = Game.Window.Size.X * coords.X,
                Y = Game.Window.Size.Y * coords.Y
            };
        } else {
            bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
            float width = largerRatio ? WorldFocusedSize.Y * Game.AspectRatio : WorldFocusedSize.X;
            float height = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;

            coords -= Position;
            return new Vector2f() {
                X = (coords.X / width + 0.5f) * Game.Window.Size.X,
                Y = (coords.Y / height + 0.5f) * Game.Window.Size.Y
            };
        }
    }

    public Vector2f WorldToScreenCoords(Vector2f worldCoords) {
        bool largerRatio = Game.AspectRatio >= FocusedAspectRatio;
        float width = largerRatio ? WorldFocusedSize.Y * Game.AspectRatio : WorldFocusedSize.X;
        float height = largerRatio ? WorldFocusedSize.Y : WorldFocusedSize.X / Game.AspectRatio;

        worldCoords -= Position;
        return new Vector2f() {
            X = (worldCoords.X / width + 0.5f) * Game.Window.Size.X,
            Y = (worldCoords.Y / height + 0.5f) * Game.Window.Size.Y
        };

    }

    public Vector2f WorldToFullScreenCoords(Vector2f worldCoords) {
        var fullSize = FullSize;
        System.Console.WriteLine(fullSize);

        var uHDFullSize = UHDFullSize;


        return new Vector2f() {
            X = ((worldCoords.X - Position.X) / fullSize.X + 0.5f) * uHDFullSize.X,
            Y = ((worldCoords.Y - Position.Y) / fullSize.Y + 0.5f) * uHDFullSize.Y
        };
    }

    public Vector2f PercentageToFullScreenCoords(Vector2f percentage) {

        var uHDFullSize = UHDFullSize;

        return new Vector2f() {
            X = percentage.X * uHDFullSize.X,
            Y = percentage.Y * uHDFullSize.Y
        };
    }

    public Vector2f WorldToFocusedScreenCoords(Vector2f worldCoords) {
        return new Vector2f() {
            X = (worldCoords.X - Position.X) / WorldFocusedSize.X + 0.5f,
            Y = (worldCoords.Y - Position.Y) / WorldFocusedSize.Y + 0.5f
        };
    }
}