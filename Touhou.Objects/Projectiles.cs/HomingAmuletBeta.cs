using OpenTK.Mathematics;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;

public class HomingAmuletBeta : ParametricProjectile {
    private float velocity;
    private float radius;
    //private RectangleShape shape;

    private Player Player => player is null ? player = Scene.GetFirstEntity<Player>() : player;
    private Player player;
    private int prevSide;

    public HomingAmuletBeta(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, float velocity, float radius, Time spawnTimeOffset = default) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {
        this.velocity = velocity;
        this.radius = radius;

        // shape = new RectangleShape(new Vector2(20f, 15f));
        // shape.Origin = shape.Size / 2f;

        // shape.FillColor4 = Color;

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 7.5f, isPlayerOwned ? CollisionGroups.PlayerProjectile : CollisionGroups.OpponentProjectile));
    }


    protected override float FuncX(float t) {
        return MathF.Sin(velocity / radius * t) * MathF.Abs(radius);
    }

    protected override float FuncY(float t) {
        return -MathF.Cos(velocity / radius * t) * radius + radius;
    }

    protected override float FuncAngle(float t) {
        return velocity / radius * t;
    }

    protected override void Tick(float t) {
        if (!IsRemote) return;

        // var normal = SampleNormal(t);
        // var side = MathF.Sign(TMathF.Dot(Player.Position - Position, new Vector2(MathF.Cos(normal), MathF.Sin(normal))));

        // if (prevSide != side) {
        //     Reset(Position, SampleTangent(t));
        //     radius = MathF.Abs(radius) * side;
        // }
        // prevSide = side;

    }


    public override void Render() {
        // shape.Rotation = 180f / MathF.PI * SampleTangent(CurrentTime);
        // shape.FillColor4 = Color;
        // shape.Position = Position;
        //Game.Draw(shape, 0);
    }

}