using OpenTK.Mathematics;

namespace Touhou;

public struct Bezier {
    public Vector2 start;
    public Vector2 control;
    public Vector2 end;

    public Bezier(Vector2 start, Vector2 control, Vector2 end) {
        this.start = start;
        this.control = control;
        this.end = end;
    }

    public Vector2 Sample(float t) {
        return MathF.Pow(1f - t, 2f) * start + 2f * t * (1f - t) * control + MathF.Pow(t, 2f) * end;
    }

    public Vector2[] SampleMultiple(int count, bool ignoreEnd = false) {

        var samples = new Vector2[ignoreEnd ? count : count + 1];

        for (int i = 0; i < count; i++) {
            samples[i] = Sample(i / (float)count);
        }

        if (!ignoreEnd) samples[samples.Length - 1] = end;

        return samples;
    }
}