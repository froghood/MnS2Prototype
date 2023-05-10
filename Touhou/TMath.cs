namespace Touhou;
public static class TMathF {
    public static float Mod(float a, float b) {
        var r = a % b;
        return r < 0f ? r + b : r;
    }

    public static float NormalizeAngle(float a) {
        return Mod(a + MathF.Tau + MathF.PI, MathF.Tau) - MathF.PI;
    }
}