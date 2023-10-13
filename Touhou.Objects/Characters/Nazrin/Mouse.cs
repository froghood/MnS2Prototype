using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;



public class Mouse : Entity {

    private Func<Vector2> leaderPositionFunction;
    private LinkedList<Mouse> mice;
    private LinkedListNode<Mouse> node;

    public Vector2 SmoothLinePosition { get; private set; }
    public float SmoothLineAngle { get; private set; }
    public Vector2 CompressedLinePosition { get; private set; }
    public byte CompressedLineAngle { get; private set; }

    private float interpolationOffset;

    public Mouse(Func<Vector2> leaderPositionFunction, LinkedList<Mouse> mice) {

        this.leaderPositionFunction = leaderPositionFunction;

        this.mice = mice;
        this.node = mice.AddLast(this);

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 5f, CollisionGroup.PlayerCompanion, Hit));

    }

    private void Hit(Entity entity, Hitbox hitbox) {
        Destroy();
    }

    public override void Update() {

        SmoothLinePosition = GetSmoothLinePosition();
        CompressedLinePosition = GetCompressedLinePosition();

        //Position = CompressedLinePosition;

        //Position = GetLinePosition(interpolationOffset);





        interpolationOffset *= 0.999f;

    }

    public override void Render() {
        var circle = new Circle {
            Radius = 4f,

            FillColor = Color4.Transparent,
            StrokeColor = new Color4(0f, 1f, 1f, 0.6f),
            StrokeWidth = 1f,

            Origin = new Vector2(0.5f),
            Position = SmoothLinePosition
        };

        var compressedCircle = new Circle {
            Radius = 8f,
            FillColor = Color4.Transparent,
            StrokeColor = new Color4(1f, 1f, 1f, 0.6f),
            StrokeWidth = 1f,

            Origin = new Vector2(0.5f),
            Position = CompressedLinePosition
        };

        Game.Draw(circle, Layer.Player);
        Game.Draw(compressedCircle, Layer.Player);
    }

    public override void Destroy() {

        var next = node.Next;
        mice.Remove(node);
        next?.Value.Reposition();

        base.Destroy();
    }

    private void Reposition() {
        interpolationOffset = (Position - GetSmoothLinePosition()).Length;
        Log.Info(interpolationOffset);

        //throw new Exception("breakpoint lol");
    }

    private Vector2 GetSmoothLinePosition() {

        var otherPosition = node.Previous == null ? leaderPositionFunction() : node.Previous.Value.SmoothLinePosition;

        var diff = SmoothLinePosition - otherPosition;

        SmoothLineAngle = MathF.Atan2(diff.Y, diff.X);

        var offset = new Vector2(MathF.Cos(SmoothLineAngle), MathF.Sin(SmoothLineAngle)) * 40f;

        return otherPosition + offset;
    }

    private Vector2 GetCompressedLinePosition() {
        var otherPosition = node.Previous == null ? leaderPositionFunction() : node.Previous.Value.SmoothLinePosition;

        var diff = SmoothLinePosition - otherPosition;

        CompressedLineAngle = (byte)MathF.Round(TMathF.NormalizeAngleZeroToTau(MathF.Atan2(diff.Y, diff.X)) / MathF.Tau * 256f);

        var angle = CompressedLineAngle / 256f * MathF.Tau;

        var offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 40f;

        return otherPosition + offset;
    }

    internal void RecaluclateSmoothPosition(Vector2 leaderPosition, float angle) {
        var otherPosition = node.Previous == null ? leaderPosition : node.Previous.Value.SmoothLinePosition;

        SmoothLineAngle = angle;

        var offset = new Vector2(MathF.Cos(SmoothLineAngle), MathF.Sin(SmoothLineAngle)) * 40f;

        SmoothLinePosition = otherPosition + offset;
    }
}