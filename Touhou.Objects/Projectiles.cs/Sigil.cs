using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class Sigil : ParametricProjectile {
    private Vector2 distance;
    private readonly Time arrivalTime;
    private readonly Time destroyTime;


    public Sigil(Vector2 startPosition, Vector2 endPosition, Time arrivalTime, Time destroyTime, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(startPosition, 0f, isP1Owned, isPlayerOwned, isRemote) {

        distance = endPosition - startPosition;

        this.arrivalTime = arrivalTime;
        this.destroyTime = destroyTime;

    }

    protected override Vector2 PositionFunction(float t) {
        return distance * Easing.Out(MathF.Min(t / arrivalTime.AsSeconds(), 1f), 3f);
    }

    public override void Update() {
        base.Update();

        if (FuncTime >= arrivalTime + destroyTime) Destroy(Time.InSeconds(0.25f));
    }

    public override void Render() {

        var sprite = new Sprite("sigil") {
            Origin = new Vector2(0.5f),
            Position = Position,
            Scale = new Vector2(0.4f),
            Rotation = Game.Time.AsSeconds() * MathF.PI,
            Color = new Color4(
                Color.R,
                Color.G,
                Color.B,
                Color.A * 0.7f * (1f - DestroyedFactor)
            ),
            UseColorSwapping = true,

        };

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);
    }
}