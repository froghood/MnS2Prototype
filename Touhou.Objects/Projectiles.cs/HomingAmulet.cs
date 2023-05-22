using SFML.System;

namespace Touhou.Objects;

public class HomingAmulet : Projectile {
    private readonly float angle;
    private readonly float radius;
    private readonly float velocity;

    public HomingAmulet(bool isRemote, Vector2f position, float angle, float radius, float velocity, Time spawnTimeOffset = default) : base(isRemote, spawnTimeOffset) {
        Position = position;
        this.angle = angle;
        this.radius = radius;
        this.velocity = velocity;

    }



    public override void Update() {





        base.Update();
    }

}