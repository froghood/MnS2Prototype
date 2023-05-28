
namespace Touhou;

public class Easing {
    public static float In(float n, float degree) => MathF.Pow(n, degree);
    public static float Out(float n, float degree) => 1f - MathF.Pow(1f - n, degree);
    public static float InOut(float n, float degree) => (n < 0.5f ? MathF.Pow(n * 2f, degree) : 2f - MathF.Pow(2f - n * 2f, degree)) / 2f;
}