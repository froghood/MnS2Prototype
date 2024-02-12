
using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Projectiles;

public class Needle : ParametricProjectile {

    private Sprite sprite;
    private float velocity;

    public Needle(Vector2 origin, float direction, float velocity, bool isP1Owned, bool isPlayerOwned, bool isRemote) : base(origin, direction, isP1Owned, isPlayerOwned, isRemote) {

        this.velocity = velocity;

        Hitboxes.Add(new CircleHitbox(this, new Vector2(0f, 0f), 6f, isP1Owned ? CollisionGroup.P1MinorProjectile : CollisionGroup.P2MinorProjectile));

        sprite = new Sprite("needle2") {
            Origin = new Vector2(0.75f, 0.5f),
            Rotation = Orientation,
            UseColorSwapping = true,
        };
    }



    protected override Vector2 PositionFunction(float t) {
        return new Vector2(t * velocity, 0f);
    }



    public override void Render() {

        sprite.Position = Position;
        sprite.Scale = new Vector2(0.38f) * (1f + 3f * (1f - SpawnFactor));
        sprite.Color = new Color4(
           Color.R,
           Color.G,
           Color.B,
           Color.A * SpawnFactor * (1f - DestroyedFactor));

        Game.Draw(sprite, IsPlayerOwned ? Layer.PlayerProjectiles : Layer.OpponentProjectiles);
    }
}

