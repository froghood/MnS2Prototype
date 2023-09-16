using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class ReimuBombWave : ParametricProjectile {

    public float Velocity { get; init; }
    public ReimuBombWave(Vector2 origin, float direction, bool isPlayerOwned, bool isRemote, Time spawnTimeOffset = default) : base(origin, direction, isPlayerOwned, isRemote, spawnTimeOffset) {

    }

    protected override float FuncX(float t) {
        return t * Velocity;
    }

    protected override float FuncY(float t) {
        return 0f;
    }

    public override void Render() {

        var alpha = MathF.Min(SpawnDelay.AsSeconds(), CurrentTime) / SpawnDelay.AsSeconds() * Color.A;
        //System.Console.WriteLine(alpha);


        var sprite = new Sprite("reimubombwave") {
            Origin = new Vector2(0f, 0.5f),
            Position = Position,
            Rotation = Direction,
            Scale = new Vector2(1f, 20f) * 0.7f,
            Color = new Color4(Color.R, Color.G, Color.B, alpha),
            UseColorSwapping = false,
            BlendMode = BlendMode.Additive,
        };

        Game.Draw(sprite, Layers.PlayerProjectiles1);


        base.Render();
    }

    public override void Destroy() {

        System.Console.WriteLine("t");
        base.Destroy();
    }
}