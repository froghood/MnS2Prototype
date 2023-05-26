using SFML.System;

namespace Touhou;
public static class TMathF {
    // public static float Mod(float a, float b) {
    //     var r = a % b;
    //     return r < 0f ? r + b : r;
    // }

    public static float Mod(float n, float d) {
        return ((n % d) + d) % d;
    }

    public static float NormalizeAngle(float a) {
        return Mod(a + MathF.PI, MathF.Tau) - MathF.PI;
    }

    public static float NormalizeAngleZeroToTwoPi(float a) {
        return Mod(a, MathF.Tau);
    }

    public static float Dot(Vector2f a, Vector2f b) {
        return a.X * b.X + a.Y + b.Y;
    }

    public static float radToDeg(float a) => a * 180f / MathF.PI;
    public static float degToRad(float a) => a * MathF.PI / 180f;
}