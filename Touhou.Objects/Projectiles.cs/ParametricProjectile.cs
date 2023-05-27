using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;



public abstract class ParametricProjectile : Projectile, IReceivable {

    public Vector2f Origin { get; private set; }

    public float Direction { get; private set; }

    public float CurrentTime => realTime;

    public Time SpawnDelay { get; init; }

    public float InterpolatedOffset { get; init; }

    public Vector2f PrevPosition { get; private set; }



    private float preCos;
    private float preSin;
    private float realTime;



    protected ParametricProjectile(Vector2f origin, float direction, bool isRemote, Time spawnTimeOffset = default(Time)) : base(isRemote, spawnTimeOffset) {
        Origin = origin;
        Direction = direction;

        Position = Origin;
        this.preCos = MathF.Cos(Direction);
        this.preSin = MathF.Sin(Direction);
    }



    protected abstract float FuncX(float t);
    protected abstract float FuncY(float t);
    protected virtual float FuncAngle(float t) => 0f;
    protected virtual Vector2f Adjust(float t, Vector2f position) => position;
    protected virtual void Tick(float t) { }



    public sealed override void Update() {
        PrevPosition = Position;

        var lifeTime = MathF.Max((Game.Time - SpawnTime).AsSeconds() - SpawnDelay.AsSeconds(), 0f);

        realTime = lifeTime + EaseOutCubic(Math.Clamp(lifeTime / 2f, 0f, 1f)) * InterpolatedOffset;

        //var realTime = InterpolateOffset ? Time + EaseOutSine(Math.Clamp(Time / 2f, 0f, 1f)) * TimeOffset : Time + TimeOffset;
        Position = Adjust(realTime, SamplePosition(realTime));
        Tick(realTime);

        base.Update();

    }



    protected Vector2f SamplePosition(float t) {
        var x = FuncX(t);
        var y = FuncY(t);
        return Origin + new Vector2f(preCos * x - preSin * y, preSin * x + preCos * y);
    }

    protected float SampleTangent(float t) => Direction + FuncAngle(t);

    protected float SampleNormal(float t) => Direction + FuncAngle(t) + MathF.PI / 2f;



    private float EaseOutSine(float t) => MathF.Sin(t * MathF.PI / 2f);
    private float EaseOutQuad(float t) => 2f * t - t * t;
    private float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);



    // private abstract class WrappingProjectile : Projectile {


    //     protected float WrapTimeLimit { get; private init; }

    //     private Vector2f _wrapLimitOffset;

    //     protected WrappingProjectile(Vector2f startingPosition, float direction, float wrapTimeLimit = -1f) : base(startingPosition, direction) {
    //         WrapTimeLimit = wrapTimeLimit;
    //     }

    //     protected override Vector2f Adjust(float time, Vector2f position) {
    //         if (WrapTimeLimit >= 0f && time >= WrapTimeLimit) {
    //             if (_wrapLimitOffset == null) {
    //                 var positionSample = SamplePosition(WrapTimeLimit);
    //                 _wrapLimitOffset = new Vector2f(
    //                     MathF.Floor(positionSample.X / Game.Window.Size.X),
    //                     MathF.Floor(positionSample.Y / Game.Window.Size.Y)
    //                 );
    //             }
    //             return new Vector2f(
    //                 position.X - _wrapLimitOffset.X * Game.Window.Size.X,
    //                 position.Y - _wrapLimitOffset.Y * Game.Window.Size.Y
    //             );
    //         }
    //         return new Vector2f(TMathF.Mod(position.X, Game.Window.Size.X), TMathF.Mod(position.Y, Game.Window.Size.Y));
    //     }
    // }

    // private abstract class BouncingProjectile : Projectile {
    //     protected BouncingProjectile(Vector2f startingPosition, float direction) : base(startingPosition, direction) { }
    //     protected override Vector2f Adjust(float time, Vector2f position) {
    //         var screenOffset = new Vector2f(
    //             MathF.Abs(MathF.Floor(position.X / Game.Window.Size.X)),
    //             MathF.Abs(MathF.Floor(position.Y / Game.Window.Size.Y))
    //         );

    //         return new Vector2f(
    //             screenOffset.X % 2 * Game.Window.Size.X + TMathF.Mod(position.X, Game.Window.Size.X) * MathF.Pow(-1, screenOffset.X),
    //             screenOffset.Y % 2 * Game.Window.Size.Y + TMathF.Mod(position.Y, Game.Window.Size.Y) * MathF.Pow(-1, screenOffset.Y)
    //         );
    //     }
    // }


    // public class Bullet : WrappingProjectile {

    //     private float _speed;
    //     private RectangleShape _shape;
    //     private float _a;
    //     private int _flipped;
    //     private Color _color;
    //     private float _s;

    //     //private float c = 4f / 3f * (MathF.Sqrt(2f) - 1f);

    //     public Bullet(Vector2f startingPosition, float direction, float speed, int flipped, Color color) : base(startingPosition, direction, 2f) {
    //         _speed = speed;

    //         _shape = new RectangleShape(new Vector2f(20f, 15f));
    //         _shape.Origin = new Vector2f(_shape.Size.X / 2f, _shape.Size.Y / 2f);
    //         _shape.FillColor = color;

    //         _a = MathF.Tau / 3f;
    //         _s = 1f;

    //         _flipped = flipped;
    //         _color = color;
    //     }

    //     protected override float FuncX(float t) {
    //         //return (MathF.Sin(MathF.Asin(MathF.Min(t - 1, 0))) + t / 4f) * _speed;
    //         //return t * _speed;
    //         return _speed * (MathF.Sin(MathF.Min(t / _s, _a)) + (MathF.Max(t / _s - _a, 0) * MathF.Cos(_a))) * _s;

    //         // x = 3c(1-t)^2*t+3(1-t)t^2+t^3
    //         //return _speed * (3f * c * MathF.Pow(1f - t, 2f) * t + 3f * (1f - t) * MathF.Pow(t, 2f) + MathF.Pow(t, 3f));

    //     }

    //     protected override float FuncY(float t) {
    //         return _flipped * _speed * (-MathF.Cos(MathF.Min(t / _s, _a)) + (MathF.Max(t / _s - _a, 0) * MathF.Sin(_a)) + 1f) * _s;

    //         //y = (1 - t) ^ 3 + 3(1 - t) ^ 2 * t + 3c(1 - t)t ^ 2 - 1
    //         //return _flipped * _speed * (MathF.Pow(1f - t, 3f) + 3f * MathF.Pow(1f - t, 2f) * t + 3f * c * (1f - t) * MathF.Pow(t, 2f) - 1f);
    //     }

    //     public override void Render() {
    //         _shape.Position = Position;
    //         _shape.Rotation = 180f / MathF.PI * MathF.Atan2(Position.Y - PrevPosition.Y, Position.X - PrevPosition.X);
    //         Game.Window.Draw(_shape);
    //     }
    // }
}

