using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects.Characters;

namespace Touhou.Objects.Projectiles;



public abstract class ParametricProjectile : Projectile, IReceivable {



    public Vector2 Origin { get; private set; }
    public float Orientation { get; private set; }
    public Time SpawnDelay { get; init; }



    private Time timeOffset;
    private Time interpolationOffset;
    private float interpolationTime;



    private Matrix2 orientationMatrix;



    protected ParametricProjectile(Vector2 origin, float orientation, bool isPlayerOwned, bool isRemote) : base(isPlayerOwned, isRemote) {
        Origin = origin;
        Orientation = orientation;

        Position = Origin;
        orientationMatrix = Matrix2.CreateRotation(orientation);
    }



    protected abstract Vector2 PositionFunction(float t);
    protected virtual Vector2 SecondaryPositionFunction(float t, Vector2 position) => position;
    protected virtual float AngleFunction(float t) => 0f;

    protected virtual void Tick(float t) { }



    public override void Update() {

        var easingFactor = Easing.In(interpolationTime, 2f);
        interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);


        float funcTime = Time.Max(LifeTime + timeOffset + interpolationOffset * easingFactor - SpawnDelay, 0L).AsSeconds();


        Position = SecondaryPositionFunction(funcTime, Origin + PositionFunction(funcTime) * orientationMatrix);
        Tick(funcTime);

        base.Update();

    }

    public float GetTangent(float t) => AngleFunction(t) + Orientation;

    public void SetTime(Time amount, bool interpolate) {
        if (interpolate) {
            var easingFactor = Easing.In(interpolationTime, 2f);
            var currentTime = LifeTime + timeOffset + interpolationOffset * easingFactor;

            timeOffset = -LifeTime + amount;

            var newTime = LifeTime + timeOffset;

            interpolationOffset = newTime - currentTime;
            interpolationTime = 1f;
        } else {
            timeOffset = -LifeTime + amount;
            interpolationOffset = 0L;
            interpolationTime = 0f;
        }

    }

    public void IncreaseTime(Time amount, bool interpolate) {
        if (interpolate) {
            var easingFactor = Easing.In(interpolationTime, 2f);
            var currentTime = LifeTime + timeOffset + interpolationOffset * easingFactor;

            timeOffset += amount;

            var newTime = LifeTime + timeOffset;

            interpolationOffset = currentTime - newTime;
            interpolationTime = 1f;
        } else {
            timeOffset += amount;
            interpolationOffset = 0L;
            interpolationTime = 0f;
        }
    }



    // private abstract class WrappingProjectile : Projectile {


    //     protected float WrapTimeLimit { get; private init; }

    //     private Vector2 _wrapLimitOffset;

    //     protected WrappingProjectile(Vector2 startingPosition, float direction, float wrapTimeLimit = -1f) : base(startingPosition, direction) {
    //         WrapTimeLimit = wrapTimeLimit;
    //     }

    //     protected override Vector2 Adjust(float time, Vector2 position) {
    //         if (WrapTimeLimit >= 0f && time >= WrapTimeLimit) {
    //             if (_wrapLimitOffset == null) {
    //                 var positionSample = SamplePosition(WrapTimeLimit);
    //                 _wrapLimitOffset = new Vector2(
    //                     MathF.Floor(positionSample.X / Game.Window.Size.X),
    //                     MathF.Floor(positionSample.Y / Game.Window.Size.Y)
    //                 );
    //             }
    //             return new Vector2(
    //                 position.X - _wrapLimitOffset.X * Game.Window.Size.X,
    //                 position.Y - _wrapLimitOffset.Y * Game.Window.Size.Y
    //             );
    //         }
    //         return new Vector2(TMathF.Mod(position.X, Game.Window.Size.X), TMathF.Mod(position.Y, Game.Window.Size.Y));
    //     }
    // }

    // private abstract class BouncingProjectile : Projectile {
    //     protected BouncingProjectile(Vector2 startingPosition, float direction) : base(startingPosition, direction) { }
    //     protected override Vector2 Adjust(float time, Vector2 position) {
    //         var screenOffset = new Vector2(
    //             MathF.Abs(MathF.Floor(position.X / Game.Window.Size.X)),
    //             MathF.Abs(MathF.Floor(position.Y / Game.Window.Size.Y))
    //         );

    //         return new Vector2(
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
    //     private Color4 _Color4;
    //     private float _s;

    //     //private float c = 4f / 3f * (MathF.Sqrt(2f) - 1f);

    //     public Bullet(Vector2 startingPosition, float direction, float speed, int flipped, Color4 Color4) : base(startingPosition, direction, 2f) {
    //         _speed = speed;

    //         _shape = new RectangleShape(new Vector2(20f, 15f));
    //         _shape.Origin = new Vector2(_shape.Size.X / 2f, _shape.Size.Y / 2f);
    //         _shape.FillColor4 = Color4;

    //         _a = MathF.Tau / 3f;
    //         _s = 1f;

    //         _flipped = flipped;
    //         _Color4 = Color4;
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
    //         Game.Draw(_shape, 0);
    //     }
    // }
}

