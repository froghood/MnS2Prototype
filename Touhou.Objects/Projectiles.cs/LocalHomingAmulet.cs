using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;

namespace Touhou.Objects.Projectiles;

public class LocalHomingAmulet : Projectile {



    private readonly float turnRadius;
    private readonly float velocity;

    private float angle;
    private int side = 0;
    private Vector2f turnPosition;



    private Vector2f visualOffset;
    private float interpolationTime;
    private bool isHoming = true;
    private bool hasStartedMoving;
    private float rotation;



    public LocalHomingAmulet(Vector2f position, float startingAngle, float turnRadius, float velocity, float hitboxRadius) : base(false) {
        Position = position;
        angle = startingAngle;
        this.turnRadius = turnRadius;
        this.velocity = velocity;


        Hitboxes.Add(new CircleHitbox(this, new Vector2f(0f, 0f), hitboxRadius));

        rotation = Game.Random.NextSingle() * 360f;
    }

    public override void Update() {


        var lifeTime = Game.Time - SpawnTime;


        if (!hasStartedMoving && lifeTime < Time.InSeconds(0.25f)) return;


        if (!isHoming) {
            Position += new Vector2f(MathF.Cos(angle), MathF.Sin(angle)) * velocity * Game.Delta.AsSeconds();
            base.Update();
            return;
        }


        if (side == 0) {
            Position += new Vector2f(MathF.Cos(angle), MathF.Sin(angle)) * velocity * Game.Delta.AsSeconds();
        } else {
            var maxTurn = velocity * Game.Delta.AsSeconds() / (turnRadius * MathF.Tau) * MathF.Tau;

            angle += maxTurn * side;

            Position = turnPosition + new Vector2f() {
                X = turnRadius * MathF.Cos(angle - MathF.PI / 2f * side),
                Y = turnRadius * MathF.Sin(angle - MathF.PI / 2f * side)
            };
        }
    }


    public override void Receive(Packet packet, IPEndPoint endPoint) {

        base.Receive(packet, endPoint);


        if (packet.Type != PacketType.UpdateProjectile) return;

        packet.Out(out uint id, true);

        if (id != Id) return;

        packet.Out(out bool theirIsHoming);

        var visualPosition = Position + visualOffset * interpolationTime;

        if (theirIsHoming) {
            packet.Out(out Time theirTime).Out(out Vector2f theirPosition).Out(out float theirAngle).Out(out int theirSide);

            Position = theirPosition;
            angle = theirAngle;
            side = theirSide;

            var latency = Game.Network.Time - theirTime;

            if (side == 0) {
                Position += new Vector2f(MathF.Cos(angle), MathF.Sin(angle)) * velocity * latency.AsSeconds();
            } else {
                turnPosition = Position + new Vector2f() {
                    X = turnRadius * MathF.Cos(angle + MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle + MathF.PI / 2f * side)
                };

                var maxTurn = velocity * latency.AsSeconds() / (turnRadius * MathF.Tau) * MathF.Tau;
                angle += maxTurn * side;
                Position = turnPosition + new Vector2f() {
                    X = turnRadius * MathF.Cos(angle - MathF.PI / 2f * side),
                    Y = turnRadius * MathF.Sin(angle - MathF.PI / 2f * side)
                };
            }
        } else {
            isHoming = false;

            packet.Out(out Time theirTime).Out(out Vector2f theirPosition).Out(out float theirAngle);





            Position = theirPosition;
            angle = theirAngle;

            var latency = Game.Network.Time - theirTime;

            System.Console.WriteLine($"{latency},{theirPosition}, {theirAngle}");

            Position += new Vector2f(MathF.Cos(angle), MathF.Sin(angle)) * velocity * latency.AsSeconds();
        }

        visualOffset = visualPosition - Position;
        interpolationTime = 1f;

        hasStartedMoving = true;
    }

    public override void Render() {

        rotation += 360f * Game.Delta.AsSeconds() * 2f;

        var states = new SpriteStates() {
            Origin = new Vector2f(0.5f, 0.5f),
            Position = Position + visualOffset * interpolationTime,
            Rotation = rotation,
            Scale = new Vector2f(1f, 1f) * 0.4f
        };

        var shader = new TShader("projectileColor");
        shader.SetUniform("color", Color);

        Game.DrawSprite("spinningamulet", states, shader, Layers.Projectiles1);


        // if (side == 0) return;

        // circle.Radius = turnRadius;
        // circle.Position = turnPosition;
        // circle.Origin = new Vector2f(1f, 1f) * circle.Radius;
        // circle.FillColor = Color.Transparent;
        // circle.OutlineColor = new Color(255, 255, 255, 30);
        // circle.OutlineThickness = 1f;
        // Game.Draw(circle, 0);
    }

    public override void PostRender() {
        interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);
    }

    public override void Destroy() {
        base.Destroy();

        System.Console.WriteLine("destroyed");
    }
}