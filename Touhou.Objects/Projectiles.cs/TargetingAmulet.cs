using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Scenes.Match.Objects;

namespace Touhou.Objects;

public class TargetingAmulet : ParametricProjectile {
    private readonly float seekingTime = 1.5f;
    private readonly float velocity;
    private readonly float deceleration;


    private readonly RectangleShape shape;

    public TargetingAmulet(Vector2f origin, float direction, bool isRemote, float velocity, float deceleration, Time spawnTimeOffset = default) : base(origin, direction, isRemote, spawnTimeOffset) {
        this.velocity = velocity;
        this.deceleration = deceleration;

        shape = new RectangleShape(new Vector2f(20f, 15f));
        shape.Origin = shape.Size / 2f;
        shape.Rotation = 180f / MathF.PI * Direction;
        shape.FillColor = Color;

        CollisionType = CollisionType.Projectile;
        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), 7.5f));

    }

    protected override float FuncX(float t) {
        return MathF.Max((velocity - t * deceleration) * t, 0f) + MathF.Min((deceleration * t * t) / 2f, MathF.Pow(velocity, 2f) / deceleration / 2f);
    }

    protected override float FuncY(float t) {
        return 0f;
    }

    // protected override void Tick(float t) {
    //     if (t >= seekingTime && IsRemote) {
    //         Destroy();

    //         var timeOverflow = Time.InSeconds(t - seekingTime);

    //         var player = Scene.GetFirstEntity<Player>();
    //         var opponent = Scene.GetFirstEntity<Opponent>();

    //         var angle = MathF.Atan2(player.Position.Y - Position.Y, player.Position.X - Position.X);

    //         var projectile = new LinearAmulet(Position, angle, false, timeOverflow) {
    //             GrazeAmount = 1,
    //             Color = Color,
    //             StartingVelocity = 300f,
    //             GoalVelocity = 300f,
    //         };
    //         if (Grazed) projectile.Graze();

    //         opponent.Scene.AddEntity(projectile);

    //         var packet = new Packet(PacketType.UpdateProjectile).In(Id ^ 0x80000000).In(Game.Network.Time - timeOverflow).In(Position).In(angle);
    //         Game.Network.Send(packet);


    //     }
    // }

    public override void Render() {
        shape.FillColor = Color;
        shape.Position = Position;
        Game.Window.Draw(shape);
    }

    // public override void Receive(Packet packet, IPEndPoint endPoint) {
    //     base.Receive(packet, endPoint);
    //     if (packet.Type != PacketType.UpdateProjectile) return;

    //     packet.Out(out uint id, true);

    //     if (Id != id) return;

    //     Destroy();

    //     packet.Out(out Time theirTime).Out(out Vector2f position).Out(out float angle);
    //     var delta = Game.Network.Time - theirTime;

    //     var player = Scene.GetFirstEntity<Player>();

    //     var projectile = new LinearAmulet(Position, angle, true) {

    //         InterpolatedOffset = delta.AsSeconds(),
    //         CanCollide = false,
    //         Color = Color,
    //         StartingVelocity = 300f,
    //         GoalVelocity = 300f,
    //     };


    //     player.Scene.AddEntity(projectile);
    // }


    public void LocalTarget(Vector2f targetPosition, Time timeOverflow) {
        Destroy();
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new LinearAmulet(Position, angle, false, timeOverflow) {
            GrazeAmount = 1,
            Color = Color,
            StartingVelocity = 300f,
            GoalVelocity = 300f,
        };
        projectile.CollisionGroups.Add(1);
        if (Grazed) projectile.Graze();

        Scene.AddEntity(projectile);
    }

    public void RemoteTarget(Time theirTime, Vector2f targetPosition) {
        Destroy();


        var delta = Game.Network.Time - theirTime;
        var angle = MathF.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);

        var projectile = new LinearAmulet(Position, angle, true) {
            InterpolatedOffset = delta.AsSeconds(),
            CanCollide = false,
            Color = Color,
            StartingVelocity = 300f,
            GoalVelocity = 300f,
        };

        Scene.AddEntity(projectile);
    }
}