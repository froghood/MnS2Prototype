using OpenTK.Mathematics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class OpponentNazrin : Opponent {

    private List<Vector2> pathHistory = new();

    private List<Mouse> mice = new();

    public OpponentNazrin(bool isP1) : base(isP1) {

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

    public override void Update() {
        base.Update();

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

        if (spline.Length > mice.Count * 40f * 3) {
            pathHistory.RemoveAt(0);
        }


        for (int i = 0; i < mice.Count; i++) {
            Mouse mouse = mice[i];

            mouse.SetPosition(spline.Sample((i + 1) * 40f));

        }
    }

    public override void Render() {

        if (IsDead) return;

        base.Render();
    }

    protected override void VelocityChanged(Packet packet) {
        base.VelocityChanged(packet);

        pathHistory.Add(basePosition);
    }

    private Vector2[] GetControlPoints() {


        var points = new Vector2[pathHistory.Count + 1];
        pathHistory.CopyTo(points);
        points[pathHistory.Count] = Position;

        return points;
    }

    private static Vector2[] GenerateSmoothTurn(Vector2 a, Vector2 b, Vector2 c) {

        var aLength = (a - b).LengthFast;
        var cLength = (c - b).LengthFast;

        var minLength = MathF.Min(MathF.Min(aLength, cLength) / 2f, 200f);

        var start = b + (a - b) / aLength * minLength;
        var end = b + (c - b) / cLength * minLength;

        return new Bezier(start, b, end).SampleMultiple(5);
    }
}

