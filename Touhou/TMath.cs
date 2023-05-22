using SFML.System;

namespace Touhou;
public static class TMathF {
    public static float Mod(float a, float b) {
        var r = a % b;
        return r < 0f ? r + b : r;
    }

    public static float NormalizeAngle(float a) {
        return Mod(a + MathF.Tau + MathF.PI, MathF.Tau) - MathF.PI;
    }

    public static float Dot(Vector2f a, Vector2f b) {
        return a.X * b.X + a.Y + b.Y;
    }
}