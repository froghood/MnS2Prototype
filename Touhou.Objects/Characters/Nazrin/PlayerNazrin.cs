using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {

    public ReadOnlySpan<Mouse> Mice { get => mice.ToArray(); }



    private List<Vector2> positionHistory = new();
    private List<Mouse> mice = new();
    private float sudoDistanceTraveled;
    private Spline spline;
    private float spacing = 60f;
    private float totalDistanceTraveled;

    public PlayerNazrin(bool isP1) : base(isP1) {


        Speed = 325f;
        FocusedSpeed = 125f;

        AddAttack(PlayerActions.Primary, new NazrinPrimary());
        AddAttack(PlayerActions.Secondary, new NazrinSecondary());
        AddAttack(PlayerActions.SpecialA, new NazrinSpecialA());
        AddAttack(PlayerActions.SpecialB, new NazrinSpecialB());

        AddBomb(new ReimuBomb());



    }

    public override void Init() {



        base.Init();

        for (int i = 0; i < 20; i++) {
            var mouse = new Mouse();
            mice.Add(mouse);
            Scene.AddEntity(mouse);
        }

        positionHistory.Add(Position);
        positionHistory.Add(Position);
        positionHistory.Add(Position);

    }

    protected override void UpdateMovement() {
        base.UpdateMovement();

        //sudoDistanceTraveled += (Velocity * Game.Delta.AsSeconds()).LengthFast;
        // if (sudoDistanceTraveled >= 100f) {

        //     Log.Info($"Saving position: {Position}");

        //     sudoDistanceTraveled = 0f;
        //     positionHistory.Add(Position);
        // }

        var controlPoints = GetControlPoints();

        var previousLength = spline.Length;

        spline = new Spline(controlPoints, c => {
            var points = new List<Vector2>();
            points.Add(c[0]);

            for (int i = 1; i < c.Length - 1; i++) {
                points.AddRange(GenerateSmoothTurn(c[i - 1], c[i], c[i + 1]));
            }

            points.Add(c[c.Length - 1]);
            points.Reverse();

            return points.ToArray();
        });

        var lengthDelta = spline.Length - previousLength;

        // spline = new Spline(controlPoints, c => {
        //     var points = new List<Vector2>();

        //     points.Add(c[0]);

        //     for (double i = 0; i < 1; i += 0.001) {
        //         points.Add(Interpolate(i, 3, c, null, null, null));
        //     }

        //     points.Add(c[c.Length - 1]);
        //     points.Reverse();

        //     return points.ToArray();

        // });

        if (Game.IsActionPressed(PlayerActions.Focus)) {
            spacing = spacing - (spacing - 20f) * 0.01f;
        } else {
            spacing = MathF.Min(spacing + lengthDelta / 2f / mice.Count, 60f);
        }

        for (int i = 0; i < mice.Count; i++) {
            Mouse mouse = mice[i];

            mouse.SetPosition(spline.SamplePosition((i + 1) * spacing));
            mouse.SetTangent(spline.SampleTangent((i + 1) * spacing));

        }
    }


    public override void Render() {

        if (IsDead) return;

        for (int i = 0; i < positionHistory.Count - 1; i++) {

            var a = positionHistory[i];
            var b = positionHistory[i + 1];
            var diff = b - a;

            var rect = new Rectangle {
                Size = new Vector2(diff.LengthFast, 2f),
                FillColor = new Color4(1f, 1f, 1f, 0.3f),
                Origin = new Vector2(0f, 0.5f),
                Position = a,
                Rotation = MathF.Atan2(diff.Y, diff.X)

            };

            var circle = new Circle {
                Radius = 2f,
                FillColor = new Color4(1f, 1f, 1f, 0.3f),
                Origin = new Vector2(0.5f),
                Position = a,
            };



            Game.Draw(rect, Layer.Player);
            Game.Draw(circle, Layer.Player);
        }


        // var sp = new List<Vector2>();
        // if (pathHistory.Count > 0) sp.Add(pathHistory[0]);
        // sp.AddRange(curves);


        // if (pathHistory.Count > 1) {

        //     var previousIndex = pathHistory.Count - 1;

        //     var control = pathHistory[previousIndex];
        //     var startLength = (pathHistory[previousIndex - 1] - control).LengthFast;
        //     var endLength = (Position - control).LengthFast;

        //     var minLength = MathF.Min(MathF.Min(startLength, endLength) / 2f, 150f);

        //     var start = control + (pathHistory[previousIndex - 1] - control) / startLength * minLength;
        //     var end = control + (Position - control) / endLength * minLength;
        //     sp.AddRange(new Bezier(start, control, end).SampleMultiple(9));
        // }

        // sp.Add(Position);

        var points = spline.Points;

        for (int i = 0; i < points.Length - 1; i++) {

            var a = points[i];
            var b = points[i + 1];
            var diff = b - a;

            var rect = new Rectangle {
                Size = new Vector2(diff.LengthFast, 2f),
                FillColor = new Color4(0f, 1f, 1f, 0.2f),
                Origin = new Vector2(0f, 0.5f),
                Position = a,
                Rotation = MathF.Atan2(diff.Y, diff.X)

            };

            var circle = new Circle {
                Radius = 3f,
                FillColor = new Color4(0f, 1f, 1f, 0.2f),
                Origin = new Vector2(0.5f),
                Position = a,
            };

            Game.Draw(rect, Layer.Player);
            Game.Draw(circle, Layer.Player);
        }

        base.Render();
    }

    protected override void ChangeVelocity(Vector2 newVelocity) {

        if (newVelocity == Velocity) return;

        base.ChangeVelocity(newVelocity);



        Log.Info($"Velocity changed");

        var previous = positionHistory[positionHistory.Count - 1];

        var distance = (Position - previous).LengthFast;

        totalDistanceTraveled += distance;

        if (totalDistanceTraveled >= 100f) {
            positionHistory.Add(Position);
            totalDistanceTraveled = 0;
        }

        // Log.Info($"Distance: {distance}");

        // if (distance < 100f) return;

        // positionHistory.Add(Position);
        // if (pathHistory.Count > 2) {

        //     var i = pathHistory.Count - 2;

        //     curves.AddRange(GenerateSmoothTurn(pathHistory[i - 1], pathHistory[i], pathHistory[i + 1]));
        // }




    }

    private static Vector2[] GenerateSmoothTurn(Vector2 a, Vector2 b, Vector2 c) {
        var start = b + (a - b) / 2f;
        var end = b + (c - b) / 2f;

        return new Bezier(start, b, end).SampleMultiple(13);
    }

    private static Vector2[] GenerateFixedSmoothTurn(Vector2 a, Vector2 b, Vector2 c) {

        var aLength = (a - b).LengthFast;
        var cLength = (c - b).LengthFast;

        var minLength = MathF.Min(MathF.Min(aLength, cLength) / 2f, 200f);

        var start = b + (a - b) / aLength * minLength;
        var end = b + (c - b) / cLength * minLength;

        return new Bezier(start, b, end).SampleMultiple(13);
    }

    private static Vector2 SamplePath(Vector2[] path, float length) {

        if (path.Length == 0) throw new Exception("Path has no points");
        if (path.Length == 1) return path[0];

        for (int i = 0; i < path.Length - 1; i++) {

            var a = path[i];
            var b = path[i + 1];

            var segmentLength = (b - a).LengthFast;

            if (segmentLength > length) {
                return a + (b - a) * (length / segmentLength);
            }

            length -= segmentLength;
        }

        return path[path.Length - 1];
    }

    private Vector2[] GetControlPoints() {


        var points = new Vector2[positionHistory.Count + 1];
        positionHistory.CopyTo(points);
        points[positionHistory.Count] = Position;

        return points;
    }

    public static Vector2 Interpolate(double t, int degree, Vector2[] vectorPoints, double[] knots, double[] weights, double[] result) {



        var points = vectorPoints.Select(e => new Double[] { e.X, e.Y }).ToArray();


        var n = points.Length; // points count
        var d = points[0].Length; // point dimensionality

        if (degree < 1) {
            throw new ArgumentOutOfRangeException(nameof(degree), "order must be at least 1 (linear)");
        }
        if (degree > n - 1) {
            throw new ArgumentOutOfRangeException(nameof(degree), "order must be less than or equal to point count - 1");
        }

        if (weights == null) {
            // build weight vector
            weights = new double[n];
            for (var i = 0; i < n; i++) {
                weights[i] = 1;
            }
        }

        if (knots == null) {
            // build knot vector of length [n + degree + 1]
            knots = new double[n + degree + 1];
            for (var i = 0; i < n + degree + 1; i++) {
                knots[i] = i;
            }
        } else {
            if (knots.Length != n + degree + 1) {
                throw new ArgumentOutOfRangeException(nameof(knots), "bad knot vector length");
            }
        }

        var domain = new int[] { degree, knots.Length - 1 - degree };

        // remap t to the domain where the spline is defined
        var low = knots[domain[0]];
        var high = knots[domain[1]];
        t = t * (high - low) + low;

        if (t < low || t > high) {
            throw new InvalidOperationException("out of bounds");
        }

        int s;
        for (s = domain[0]; s < domain[1]; s++) {
            if (t >= knots[s] && t <= knots[s + 1]) {
                break;
            }
        }

        // convert points to homogeneous coordinates
        var v = new double[n, d + 1];
        for (var i = 0; i < n; i++) {
            for (var j = 0; j < d; j++) {
                v[i, j] = points[i][j] * weights[i];
            }
            v[i, d] = weights[i];
        }

        // l (level) goes from 1 to the curve order
        for (var l = 1; l <= degree + 1; l++) {
            // build level l of the pyramid
            for (var i = s; i > s - degree - 1 + l; i--) {
                var alpha = (t - knots[i]) / (knots[i + degree + 1 - l] - knots[i]);

                // interpolate each component
                for (var j = 0; j < d + 1; j++) {
                    v[i, j] = (1 - alpha) * v[i - 1, j] + alpha * v[i, j];
                }
            }
        }

        // convert back to cartesian and return
        if (result == null) {
            result = new double[d];
        }
        for (var i = 0; i < d; i++) {
            result[i] = v[s, i] / v[s, d];
        }

        return new Vector2((float)result[0], (float)result[1]);
    }
}