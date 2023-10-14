

using OpenTK.Mathematics;

namespace Touhou;

public struct Spline {

    public ReadOnlySpan<Vector2> Points { get => points; }
    public float Length { get => length; }

    private Vector2[] points;

    private float length;

    public Spline(Vector2[] controlPoints, Func<Vector2[], Vector2[]> splineFunction) {

        points = splineFunction(controlPoints);

        for (int i = 0; i < points.Length - 1; i++) {
            length += (points[i + 1] - points[i]).LengthFast;
        }
    }

    public Vector2 SamplePosition(float length) {

        if (points.Length == 0) throw new Exception("Path has no points");
        if (points.Length == 1) return points[0];

        for (int i = 0; i < points.Length - 1; i++) {

            var a = points[i];
            var b = points[i + 1];

            var segmentLength = (b - a).LengthFast;

            if (segmentLength > length) {
                return a + (b - a) * (length / segmentLength);
            }

            length -= segmentLength;
        }

        return points[points.Length - 1];
    }

    public float SampleTangent(float length) {
        if (points.Length < 2) throw new Exception("Need at least 2 points to sample a tangent");

        for (int i = 0; i < points.Length - 1; i++) {
            var a = points[i];
            var b = points[i + 1];

            var segmentLength = (b - a).LengthFast;

            if (segmentLength > length) {
                return GetVectorAngle(b - a);
            }

            length -= segmentLength;
        }

        return GetVectorAngle(points[points.Length - 2] - points[points.Length - 1]);

        float GetVectorAngle(Vector2 vector) {
            return MathF.Atan2(vector.Y, vector.X);
        }



    }
}