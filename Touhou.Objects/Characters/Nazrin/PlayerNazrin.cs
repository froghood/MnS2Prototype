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

        SpawnMouse(1);

        positionHistory.Add(Position);

    }





    public override void Render() {

        if (IsDead) return;

        //RenderPoints(controlPoints, new Color4(1f, 1f, 1f, 0.2f), Layer.UI1);
        //RenderPoints(smoothControlPoints, new Color4(0f, 1f, 0f, 0.2f), Layer.UI1);
        //RenderPoints(spline.Points.ToArray(), new Color4(0f, 1f, 1f, 0.2f), Layer.UI1);

        base.Render();
    }



    public void SpawnMouse(int count = 1) {

        for (int i = 0; i < count; i++) {
            var mouse = new Mouse();
            mice.Add(mouse);
            Scene.AddEntity(mouse);


            var angle = Game.Random.NextSingle() * MathF.Tau;
            var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 500f;

            mouse.Interpolate(offset, Time.InSeconds(1f));
        }
    }



    protected override void ChangeVelocity(Vector2 newVelocity) {

        if (newVelocity == Velocity) return;

        base.ChangeVelocity(newVelocity);

        var previous = positionHistory[positionHistory.Count - 1];

        var distance = (Position - previous).LengthFast;

        totalDistanceTraveled += distance;

        if (totalDistanceTraveled >= 40f) {
            positionHistory.Add(Position);
            totalDistanceTraveled = 0;
        }
    }



    protected override void UpdateMovement() {
        base.UpdateMovement();

        controlPoints = GetControlPoints();
        smoothControlPoints = GetSmoothControlPoints(controlPoints);



        var previousLength = spline.Length;

        spline = new Spline(smoothControlPoints, c => {

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



        if (Game.IsActionPressed(PlayerActions.Focus)) {
            spacing = spacing - (spacing - 20f) * 0.01f;
        } else {
            spacing = MathF.Min(spacing + lengthDelta / 2f / mice.Count, 50f);
        }



        for (int i = 0; i < mice.Count; i++) {
            Mouse mouse = mice[i];

            mouse.SetPosition(spline.SamplePosition((i + 1) * spacing));
            mouse.SetTangent(spline.SampleTangent((i + 1) * spacing));

        }
    }



    private Vector2[] GetControlPoints() {

        var points = new Vector2[positionHistory.Count + 1];
        positionHistory.CopyTo(points);
        points[positionHistory.Count] = Position;

        return points;
    }



    private static Vector2[] GetSmoothControlPoints(Vector2[] c) {

        var smooth = new List<Vector2>();

        smooth.Add(c[0]);

        for (int i = 0; i < c.Length; i++) {

            var start = i > 0 ? c[i - 1] : c[i];
            var middle = c[i];
            var end = i < c.Length - 1 ? c[i + 1] : c[i];

            var startLength = (start - middle).LengthFast;
            var endLength = (end - middle).LengthFast;

            var maxStartLength = MathF.Min(startLength, 250f);
            var maxEndLength = MathF.Min(endLength, 250f);

            var realStart = middle + (maxStartLength / startLength) * (start - middle);
            var realEnd = middle + (maxEndLength / endLength) * (end - middle);

            smooth.Add(new Bezier(realStart, middle, realEnd).Sample(0.5f));
        }

        smooth.Add(c[c.Length - 1]);

        return smooth.ToArray();
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
    }


}