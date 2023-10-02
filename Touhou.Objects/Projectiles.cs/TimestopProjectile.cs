using OpenTK.Mathematics;

namespace Touhou.Objects.Projectiles;

public abstract class TimestopProjectile : Projectile {

    public Time SpawnDelay { get; init; }

    public Vector2 Origin { get => origin; }
    public float Orientation { get => orientation; }
    public float Tangent { get => tangent; }

    public bool IsTimestopped { get => isTimestopped; }

    private Vector2 origin;



    private float orientation;
    private Matrix2 orientationMatrix;



    private float tangent;


    private bool startsTimeStopped;
    private bool isTimestopped;


    private Time unfreezeTime;



    private Time timeOffset;
    private Time interpolationOffset;
    private float interpolationTime;



    public TimestopProjectile(Vector2 origin, float orientation, bool startsTimeStopped, bool isPlayerOwned, bool isRemote) : base(isPlayerOwned, isRemote) {

        this.origin = origin;
        this.orientation = orientation;

        this.startsTimeStopped = startsTimeStopped;
        isTimestopped = startsTimeStopped;

        Position = origin;
        orientationMatrix = Matrix2.CreateRotation(orientation);
    }



    protected abstract Vector2 PositionFunction(float t);
    protected virtual float AngleFunction(float t) => 0f;
    protected virtual Vector2 SecondaryPositionFunction(float t, Vector2 position) => position;
    protected virtual void Tick(float t) { }



    public override void Update() {


        if (startsTimeStopped) {
            if (isTimestopped) {

                Position = Origin;
                tangent = TMathF.NormalizeAngle(orientation + AngleFunction(0f));

            } else {

                var easingFactor = Easing.In(interpolationTime, 2f);
                interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);

                float funcTime = Time.Max(LifeTime + timeOffset + interpolationOffset * easingFactor - SpawnDelay, 0L).AsSeconds();

                // accelerate factor
                funcTime -= (1f - MathF.Pow(MathF.Min(funcTime, 1f) - 1f, 2f)) / 2f;

                Position = SecondaryPositionFunction(funcTime, Origin + PositionFunction(funcTime) * orientationMatrix);
                tangent = TMathF.NormalizeAngle(orientation + AngleFunction(funcTime));
            }

        } else {

            var easingFactor = Easing.In(interpolationTime, 2f);
            interpolationTime = MathF.Max(interpolationTime - Game.Delta.AsSeconds(), 0f);

            float funcTime = Time.Max(LifeTime + timeOffset + interpolationOffset * easingFactor - SpawnDelay, 0L).AsSeconds();

            Position = SecondaryPositionFunction(funcTime, SamplePosition(funcTime));
            tangent = TMathF.NormalizeAngle(orientation + AngleFunction(funcTime));
        }

        base.Update();
    }


    protected Vector2 SamplePosition(float t) => Origin + PositionFunction(t) * orientationMatrix;

    public void Unfreeze(Time timeIncrease, bool interpolate) {
        isTimestopped = false;
        SetTime(Time.Min(LifeTime, SpawnDelay), false);
        IncreaseTime(timeIncrease, interpolate);
    }



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
}