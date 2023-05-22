using SFML.Graphics;
using SFML.System;
using Touhou.Scenes.Match.Objects;

namespace Touhou.Objects;

public class HomingAmuletBeta : ParametricProjectile {
    private float velocity;
    private float radius;
    private RectangleShape shape;

    private Player Player => player is null ? player = Scene.GetFirstEntity<Player>() : player;
    private Player player;
    private int prevSide;

    public HomingAmuletBeta(Vector2f origin, float direction, bool isRemote, float velocity, float radius, Time spawnTimeOffset = default) : base(origin, direction, isRemote, spawnTimeOffset) {
        this.velocity = velocity;
        this.radius = radius;

        shape = new RectangleShape(new Vector2f(20f, 15f));
        shape.Origin = shape.Size / 2f;

        shape.FillColor = Color;

        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 7.5f));
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
        // var side = MathF.Sign(TMathF.Dot(Player.Position - Position, new Vector2f(MathF.Cos(normal), MathF.Sin(normal))));

        // if (prevSide != side) {
        //     Reset(Position, SampleTangent(t));
        //     radius = MathF.Abs(radius) * side;
        // }
        // prevSide = side;

    }


    public override void Render() {
        shape.Rotation = 180f / MathF.PI * SampleTangent(CurrentTime);
        shape.FillColor = Color;
        shape.Position = Position;
        Game.Window.Draw(shape);
    }

}