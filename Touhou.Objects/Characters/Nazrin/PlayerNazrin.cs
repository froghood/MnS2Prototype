using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {

    public ReadOnlySpan<Mouse> Mice { get => mice.ToArray(); }

    private List<Vector2> positionHistory = new();
    private List<Mouse> mice = new();

    private float sudoDistanceTraveled;
    private Spline spline;
    private float spacing = 50f;
    private float totalDistanceTraveled;
    private Vector2[] controlPoints = { };
    private Vector2[] smoothControlPoints = { };
    private float smoothing;
    private float totalSplineLength;

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

        //SpawnMouse(1);

        positionHistory.Add(Position);

    }

    public override void Update() {

        mice = mice.Where(e => !e.IsDestroyed).ToList();

        base.Update();
    }





    public override void Render() {

        if (IsDead) return;

        RenderPoints(controlPoints, new Color4(1f, 1f, 1f, 0.2f), Layer.UI1);
        RenderPoints(smoothControlPoints, new Color4(1f, 0f, 1f, 0.2f), Layer.UI1);
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



    protected override void ChangeVelocity(Vector2 newVelocity) {

        if (newVelocity == Velocity) return;

        base.ChangeVelocity(newVelocity);

        var previous = positionHistory[positionHistory.Count - 1];

        var length = (Position - previous).LengthFast;

        totalDistanceTraveled += length;

        if (totalDistanceTraveled >= 40f) {

            totalDistanceTraveled = 0;


            // var steps = (int)MathF.Ceiling(length / 300f);

            // for (int i = 1; i < steps; i++) {
            //     var t = i * 300f / length;

            //     positionHistory.Add(previous + t * (Position - previous));
            // }
            positionHistory.Add(Position);

        }
    }



    protected override void UpdateMovement() {
        base.UpdateMovement();

        controlPoints = GetControlPoints();
        smoothControlPoints = GetSmoothControlPointsA(controlPoints);



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

        //smoothing *= MathF.Pow(0.001f, Game.Delta.AsSeconds());
        //smoothing += lengthDelta;



        for (int i = 0; i < mice.Count; i++) {
            Mouse mouse = mice[i];
            var targetSpacing = totalSplineLength - (i + 1) * (Focused ? 20f : 50f);
            mouse.SetTargetSpacing(targetSpacing, true);


            mouse.SetBasePosition(spline.SamplePosition(totalSplineLength - mouse.Spacing));
            mouse.SetSmoothPosition(spline.SamplePosition(totalSplineLength - mouse.Spacing));
            mouse.SetTangent(spline.SampleTangent(totalSplineLength - mouse.Spacing));

        }


    }



    private Vector2[] GetControlPoints() {

        var points = new List<Vector2>(positionHistory);



        // var previous = points[points.Count - 1];

        // var length = (Position - previous).LengthFast;
        // var steps = (int)MathF.Ceiling(length / 300f);

        // for (int i = 1; i < steps; i++) {
        //     var t = i * 300f / length;

        //     points.Add(previous + t * (Position - previous));
        // }

        points.Add(Position);



        return points.ToArray();
    }



    private static Vector2[] GetSmoothControlPointsA(Vector2[] c) {

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



    private static Vector2[] GetSmoothControlPointsB(Vector2[] c) {

        var smooth = new List<Vector2> { c[0] };

        for (int i = 0; i < c.Length - 1; i++) {

            var a = c[i];
            var b = c[i + 1];

            smooth.Add((a + b) / 2f);
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

        return new Bezier(start, b, end).SampleMultiple(8);
    }



    private static Vector2[] GenerateCappedSmoothTurn(Vector2 a, Vector2 b, Vector2 c) {

        var aLength = (a - b).LengthFast;
        var cLength = (c - b).LengthFast;

        var minALength = MathF.Min(aLength / 2f, 200f);
        var minCLength = MathF.Min(cLength / 2f, 200f);

        var start = b + (a - b) * (minALength / aLength);
        var end = b + (c - b) * (minCLength / cLength);

        return new Bezier(start, b, end).SampleMultiple(8);




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


