using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class Mouse : Entity {


    public float Spacing { get => smoothSpacing; }
    public Vector2 BasePosition { get => basePosition; }

    public float Tangent { get => tangent; }


    private float targetSpacing;
    private float spacing;

    private float smoothSpacing;

    private Vector2 basePosition;
    private Vector2 smoothPosition;


    private float tangent;
    private Queue<(Action<Mouse, Time> Attack, Time Time)> queuedAttacks = new();
    private Vector2 interpolationOffset;
    private Timer interpolationTimer;
    private readonly bool isPlayerOwned;

    public Mouse(bool isPlayerOwned) {
        Hitboxes.Add(new CircleHitbox(this, Vector2.Zero, 0.5f, CollisionGroup.PlayerCompanion, Hit));
        this.isPlayerOwned = isPlayerOwned;
    }



    public override void Update() {


        spacing += MathF.Min(MathF.Abs(MathF.Max(targetSpacing - spacing, 0f)), 600f * Game.Delta.AsSeconds()) * MathF.Sign(targetSpacing - spacing);

        smoothSpacing += (spacing - smoothSpacing) * (1f - MathF.Pow(0.001f, Game.Delta.AsSeconds()));




        //var interpolatedBasePosition = smoothPosition + interpolationOffset * Easing.In(interpolationTimer.RemainingRatio, 3f);
        //smoothPosition += (interpolatedBasePosition - smoothPosition) * (1f - MathF.Pow(0.00001f, Game.Delta.AsSeconds()));

        Position = smoothPosition + interpolationOffset * Easing.In(interpolationTimer.RemainingRatio, 3f);



        while (queuedAttacks.Count > 0) {

            var queuedAttack = queuedAttacks.Peek();

            if (queuedAttack.Time <= Game.Time) {
                queuedAttack.Attack.Invoke(this, Game.Time - queuedAttack.Time);

                queuedAttacks.Dequeue();
            } else {
                break;
            }

        }
    }


    public override void Render() {

        var sprite = new Sprite("mouse2") {
            Origin = new Vector2(0.5f),
            Position = Position,
            Scale = new Vector2(0.325f),
            Color = isPlayerOwned ? new Color4(0.8f, 1f, 0.8f, 1f) : new Color4(1f, 0.8f, 0.8f, 1f),
        };

        Game.Draw(sprite, Layer.Foreground1);



        var circle = new Circle() {
            Radius = 3f,
            FillColor = Color4.Transparent,
            StrokeColor = new Color4(1f, 1f, 1f, 0.5f),
            StrokeWidth = 2f,
            Origin = new Vector2(0.5f),
            Position = basePosition
        };

        //Game.Draw(circle, Layer.Foreground1);


    }

    public void SetBasePosition(Vector2 newPosition) {
        basePosition = newPosition;
    }

    public void SetSmoothPosition(Vector2 newPosition) {
        smoothPosition = newPosition;
    }

    public void SetTangent(float newTangent) => tangent = newTangent;

    public void PlayerAttack(Action<Mouse, Time> attack, Time time) {
        queuedAttacks.Enqueue((attack, time));
    }

    public void Interpolate(Vector2 offset, Time duration) {
        smoothPosition = basePosition + offset;
        interpolationOffset = offset;
        interpolationTimer = new Timer(duration);
    }

    public void SetTargetSpacing(float newSpacing, bool interpolate) {
        targetSpacing = newSpacing;

        if (!interpolate) {
            this.spacing = newSpacing;
            smoothSpacing = newSpacing;
        }
    }

    private void Hit(Entity entity, Hitbox hitbox) {
        Destroy();
    }




}