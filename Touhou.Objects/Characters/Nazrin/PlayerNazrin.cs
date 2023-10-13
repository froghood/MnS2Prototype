using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {

    private List<Vector2> pathHistory = new();
    private List<Vector2> smoothPath = new();
    public PlayerNazrin(bool isP1) : base(isP1) {


        Speed = 325f;
        FocusedSpeed = 125f;

        AddAttack(PlayerActions.Primary, new NazrinPrimary());
        AddAttack(PlayerActions.Secondary, new NazrinSecondary());
        AddAttack(PlayerActions.SpecialA, new NazrinSpecialA());
        AddAttack(PlayerActions.SpecialB, new NazrinSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        for (int i = 0; i < pathHistory.Count - 1; i++) {

            var a = pathHistory[i];
            var b = pathHistory[i + 1];
            var diff = b - a;

            // var rect = new Rectangle {
            //     Size = new Vector2(diff.LengthFast, 2f),
            //     FillColor = new Color4(1f, 1f, 1f, 0.1f),
            //     Origin = new Vector2(0f, 0.5f),
            //     Position = a,
            //     Rotation = MathF.Atan2(diff.Y, diff.X)

            // };

            // var circle = new Circle {
            //     Radius = 2f,
            //     FillColor = new Color4(1f, 1f, 1f, 0.1f),
            //     Origin = new Vector2(0.5f),
            //     Position = a,
            // };



            //Game.Draw(rect, Layer.Player);
            //Game.Draw(circle, Layer.Player);
        }


        var sp = new List<Vector2>();
        if (pathHistory.Count > 0) sp.Add(pathHistory[0]);
        sp.AddRange(smoothPath);


        if (pathHistory.Count > 1) {

            var previousIndex = pathHistory.Count - 1;

            var control = pathHistory[previousIndex];
            var startLength = (pathHistory[previousIndex - 1] - control).LengthFast;
            var endLength = (Position - control).LengthFast;

            var minLength = MathF.Min(MathF.Min(startLength, endLength) / 2.2f, 80f);

            var start = control + (pathHistory[previousIndex - 1] - control) / startLength * minLength;
            var end = control + (Position - control) / endLength * minLength;
            sp.AddRange(new Bezier(start, control, end).SampleMultiple(4));
        }

        sp.Add(Position);


        for (int i = 0; i < sp.Count - 1; i++) {

            var a = sp[i];
            var b = sp[i + 1];
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

        Log.Info($"Velocity changed");

        pathHistory.Add(Position);
        if (pathHistory.Count > 2) {

            var previousIndex = pathHistory.Count - 2;

            var control = pathHistory[previousIndex];
            var startLength = (pathHistory[previousIndex - 1] - control).LengthFast;
            var endLength = (pathHistory[previousIndex + 1] - control).LengthFast;

            var minLength = MathF.Min(MathF.Min(startLength, endLength) / 2.2f, 80f);

            var start = control + (pathHistory[previousIndex - 1] - control) / startLength * minLength;
            var end = control + (pathHistory[previousIndex + 1] - control) / endLength * minLength;

            smoothPath.AddRange(new Bezier(start, control, end).SampleMultiple(4));
        }



        base.ChangeVelocity(newVelocity);
    }
}