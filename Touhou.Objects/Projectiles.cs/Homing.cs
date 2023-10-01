using OpenTK.Mathematics;

namespace Touhou.Objects.Projectiles;

public class Homing : Projectile {

    public Time SpawnDuration { get; init; }
    public Time PreHomingDuration { get; init; }
    public Time HomingDuration { get; init; }

    protected float turnRadius;
    protected float velocity;
    protected float angle;

    protected float visualRotation;

    protected HomingState state;
    protected int side;
    protected Vector2 turnPosition;
    protected Homing(bool isPlayerOwned, bool isRemote) : base(isPlayerOwned, isRemote) { }

    protected enum HomingState : byte {
        Spawning,
        PreHoming,
        Homing,
        PostHoming,
    }

    protected enum HomingSide {
        Left = -1,
        Center = 0,
        Right = 1,
    }
}