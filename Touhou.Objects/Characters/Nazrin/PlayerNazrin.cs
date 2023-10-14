using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {

    private List<Vector2> pathHistory = new();
    private List<Vector2> curves = new();


    private List<Mouse> mice = new();

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
    }

    protected override void UpdateMovement() {
        base.UpdateMovement();

        var controlPoints = GetControlPoints();

        var spline = new Spline(controlPoints, c => {
            var points = new List<Vector2>();
            points.Add(c[0]);

            for (int i = 1; i < c.Length - 1; i++) {
                points.AddRange(GenerateSmoothTurn(c[i - 1], c[i], c[i + 1]));
            }

            points.Add(c[c.Length - 1]);
            points.Reverse();

            return points.ToArray();
        });

        for (int i = 0; i < mice.Count; i++) {
            Mouse mouse = mice[i];

            mouse.SetPosition(spline.Sample((i + 1) * 40f));

        }
    }


    public override void Render() {

        if (IsDead) return;

        // for (int i = 0; i < pathHistory.Count - 1; i++) {

        //     var a = pathHistory[i];
        //     var b = pathHistory[i + 1];
        //     var diff = b - a;

        //     var rect = new Rectangle {
        //         Size = new Vector2(diff.LengthFast, 2f),
        //         FillColor = new Color4(1f, 1f, 1f, 0.1f),
        //         Origin = new Vector2(0f, 0.5f),
        //         Position = a,
        //         Rotation = MathF.Atan2(diff.Y, diff.X)

        //     };

        //     var circle = new Circle {
        //         Radius = 2f,
        //         FillColor = new Color4(1f, 1f, 1f, 0.1f),
        //         Origin = new Vector2(0.5f),
        //         Position = a,
        //     };



        //     Game.Draw(rect, Layer.Player);
        //     Game.Draw(circle, Layer.Player);
        // }


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


        // for (int i = 0; i < sp.Count - 1; i++) {

        //     var a = sp[i];
        //     var b = sp[i + 1];
        //     var diff = b - a;

        //     var rect = new Rectangle {
        //         Size = new Vector2(diff.LengthFast, 2f),
        //         FillColor = new Color4(0f, 1f, 1f, 0.2f),
        //         Origin = new Vector2(0f, 0.5f),
        //         Position = a,
        //         Rotation = MathF.Atan2(diff.Y, diff.X)

        //     };

        //     var circle = new Circle {
        //         Radius = 3f,
        //         FillColor = new Color4(0f, 1f, 1f, 0.2f),
        //         Origin = new Vector2(0.5f),
        //         Position = a,
        //     };

        //     Game.Draw(rect, Layer.Player);
        //     Game.Draw(circle, Layer.Player);
        // }

        base.Render();
    }

    protected override void ChangeVelocity(Vector2 newVelocity) {
        if (newVelocity == Velocity) return;

        //Log.Info($"Velocity changed");

        pathHistory.Add(Position);
        // if (pathHistory.Count > 2) {

        //     var i = pathHistory.Count - 2;

        //     curves.AddRange(GenerateSmoothTurn(pathHistory[i - 1], pathHistory[i], pathHistory[i + 1]));
        // }



        base.ChangeVelocity(newVelocity);
    }

    private static Vector2[] GenerateSmoothTurn(Vector2 a, Vector2 b, Vector2 c) {

        var aLength = (a - b).LengthFast;
        var cLength = (c - b).LengthFast;

        var minLength = MathF.Min(MathF.Min(aLength, cLength) / 2f, 200f);

        var start = b + (a - b) / aLength * minLength;
        var end = b + (c - b) / cLength * minLength;

        return new Bezier(start, b, end).SampleMultiple(5);
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


        var points = new Vector2[pathHistory.Count + 1];
        pathHistory.CopyTo(points);
        points[pathHistory.Count] = Position;

        return points;
    }
}