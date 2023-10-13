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

        int _count = ignoreEnd ? count - 1 : count;

        var samples = new Vector2[_count];
        samples[0] = start;
        for (int i = 1; i < _count; i++) {
            samples[i] = Sample(i / (count - 1f));
        }
        return samples;
    }
}