using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;



public class Mouse : Entity {

    private Entity leader;
    private LinkedList<Mouse> mice;
    private LinkedListNode<Mouse> node;

    private Vector2 linePosition;
    private float interpolationOffset;

    public Mouse(Entity leader, LinkedList<Mouse> mice) {

        this.leader = leader;

        this.mice = mice;
        this.node = mice.AddLast(this);

        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 5f, CollisionGroup.PlayerCompanion, Hit));

    }

    private void Hit(Entity entity, Hitbox hitbox) {
        Destroy();
    }

    public override void Update() {

        Position = GetLinePosition(interpolationOffset);

        interpolationOffset *= 0.999f;

    }

    public override void Render() {
        var circle = new Circle {
            Radius = 8f,

            FillColor = Color4.Transparent,
            StrokeColor = new Color4(1f, 1f, 1f, 0.6f),
            StrokeWidth = 1f,

            Origin = new Vector2(0.5f),
            Position = Position
        };

        Game.Draw(circle, Layer.Player);
    }

    public override void Destroy() {

        var next = node.Next;
        mice.Remove(node);
        next?.Value.Reposition();

        base.Destroy();
    }

    private void Reposition() {
        interpolationOffset = (Position - GetLinePosition(0f)).Length;
        Log.Info(interpolationOffset);

        //throw new Exception("breakpoint lol");
    }

    private Vector2 GetLinePosition(float offset) {
        var entity = node.Previous == null ? leader : node.Previous.Value;

        var diff = Position - entity.Position;

        var angle = MathF.Atan2(diff.Y, diff.X);

        var lineOffset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (30f + offset);

        return entity.Position + lineOffset;
    }


}