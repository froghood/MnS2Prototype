using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class OpponentNazrin : Opponent {

    public ReadOnlySpan<Mouse> Mice { get => mice.ToArray(); }

    private List<Vector2> positionHistory = new();
    private List<Mouse> mice = new();

    private float totalDistanceTraveled;
    private float spacing = 50f;
    private Vector2[] controlPoints = { };
    private Vector2[] smoothControlPoints = { };
    private Spline spline;
    private float totalSplineLength;

    public OpponentNazrin(bool isP1) : base(isP1) {

        AddAttack(PlayerActions.Primary, new NazrinPrimary());
        AddAttack(PlayerActions.Secondary, new NazrinSecondary());
        AddAttack(PlayerActions.SpecialA, new NazrinSpecialA());
        AddAttack(PlayerActions.SpecialB, new NazrinSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Init() {
        base.Init();

        SpawnMouse(1);

        positionHistory.Add(basePosition);
    }

    public override void Update() {

        mice = mice.Where(e => !e.IsDestroyed).ToList();

        base.Update();

        controlPoints = GetControlPoints();
        smoothControlPoints = GetSmoothControlPoints(controlPoints);

        var previousLength = spline.Length;

        spline = new Spline(smoothControlPoints, c => {

            var points = new List<Vector2>();
            points.Add(c[0]);

            for (int i = 1; i < c.Length - 1; i++) {

                bool ignoreEnd = i < c.Length - 2;

                points.AddRange(GenerateSmoothTurn(c[i - 1], c[i], c[i + 1], ignoreEnd));
            }

            points.Add(c[c.Length - 1]);
            points.Reverse();

            return points.ToArray();
        });

        var lengthDelta = spline.Length - previousLength;
        totalSplineLength += lengthDelta;



        for (int i = 0; i < mice.Count; i++) {

            Mouse mouse = mice[i];
            var targetSpacing = totalSplineLength - (i + 1) * (false ? 20f : 50f);
            mouse.SetTargetSpacing(targetSpacing, true);


            mouse.SetBasePosition(spline.SamplePosition(totalSplineLength - mouse.Spacing));
            mouse.SetSmoothPosition(spline.SamplePosition(totalSplineLength - mouse.Spacing));
            mouse.SetTangent(spline.SampleTangent(totalSplineLength - mouse.Spacing));

        }
    }

    public override void Render() {

        if (IsDead) return;

        RenderPoints(controlPoints, new Color4(1f, 1f, 1f, 0.2f), Layer.UI1);
        RenderPoints(smoothControlPoints, new Color4(0f, 1f, 0f, 0.2f), Layer.UI1);
        RenderPoints(spline.Points.ToArray(), new Color4(0f, 1f, 1f, 0.2f), Layer.UI1);


        base.Render();
    }



    public void SpawnMouse(int count = 1) {
        for (int i = 0; i < count; i++) {
            var mouse = new Mouse(true);
            mice.Add(mouse);
            Scene.AddEntity(mouse);


            var spacing = totalSplineLength - (mice.Count * 50);
            mouse.SetTargetSpacing(spacing, false);


            var angle = Game.Random.NextSingle() * MathF.Tau;
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 500f;

            mouse.Interpolate(offset, Time.InSeconds(1f));


        }
    }



    protected override void VelocityChanged(Packet packet) {

        base.VelocityChanged(packet);

        var previous = positionHistory[positionHistory.Count - 1];

        var distance = (basePosition - previous).LengthFast;

        totalDistanceTraveled += distance;

        if (totalDistanceTraveled >= 40f) {
            positionHistory.Add(basePosition);
            totalDistanceTraveled = 0;
        }
    }



    private Vector2[] GetControlPoints() {

        var points = new List<Vector2>(positionHistory);
        points.Add(Position);

        return points.ToArray();
    }



    private static Vector2[] GetSmoothControlPoints(Vector2[] c) {

        var smooth = new List<Vector2> { c[0] };

        for (int i = 0; i < c.Length; i++) {

            var start = i > 0 ? c[i - 1] : c[i];
            var middle = c[i];
            var end = i < c.Length - 1 ? c[i + 1] : c[i];

            var startLength = (start - middle).LengthFast;
            var endLength = (end - middle).LengthFast;

            var maxStartLength = MathF.Min(startLength, 200f);
            var maxEndLength = MathF.Min(endLength, 200f);

            var realStart = middle + (maxStartLength / startLength) * (start - middle);
            var realEnd = middle + (maxEndLength / endLength) * (end - middle);

            smooth.Add(new Bezier(realStart, middle, realEnd).Sample(0.5f));
        }

        smooth.Add(c[c.Length - 1]);

        return smooth.ToArray();
    }



    private static Vector2[] GenerateSmoothTurn(Vector2 a, Vector2 b, Vector2 c, bool ignoreEnd) {
        var start = b + (a - b) / 2f;
        var end = b + (c - b) / 2f;

        return new Bezier(start, b, end).SampleMultiple(8, ignoreEnd);
    }



    private static Vector2[] GenerateFixedSmoothTurn(Vector2 a, Vector2 b, Vector2 c) {

        var aLength = (a - b).LengthFast;
        var cLength = (c - b).LengthFast;

        var minLength = MathF.Min(MathF.Min(aLength, cLength) / 2f, 200f);

        var start = b + (a - b) / aLength * minLength;
        var end = b + (c - b) / cLength * minLength;

        return new Bezier(start, b, end).SampleMultiple(13);
    }



    private static void RenderPoints(Vector2[] points, Color4 color, Layer layer) {

        for (int i = 0; i < points.Length - 1; i++) {

            var a = points[i];
            var b = points[i + 1];

            var line = new Rectangle {
                Size = new Vector2((b - a).LengthFast, 2f),
                FillColor = color,
                Origin = new Vector2(0f, 0.5f),
                Position = a,
                Rotation = MathF.Atan2((b - a).Y, (b - a).X)
            };

            Game.Draw(line, layer);
        }

        for (int i = 0; i < points.Length; i++) {
            var a = points[i];

            var node = new Rectangle {
                Size = new Vector2(6f),
                FillColor = color,
                Origin = new Vector2(0.5f),
                Position = a,
            };

            Game.Draw(node, layer);
        }
    }
}



