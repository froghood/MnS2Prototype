using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class Mouse : Entity {

    public Vector2 BasePosition { get => basePosition; }

    public float Tangent { get => tangent; }


    private Vector2 basePosition;
    private float tangent;
    private Queue<(Action<Mouse, Time> Attack, Time Time)> queuedAttacks = new();
    private Vector2 interpolationOffset;
    private Timer interpolationTimer;

    public override void Update() {


        Position = basePosition + interpolationOffset * Easing.In(interpolationTimer.RemainingRatio, 3f);



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

        var sprite = new Sprite("mouse") {
            Origin = new Vector2(0.5f),
            Position = Position,
            Scale = new Vector2(0.3f)
        };

        Game.Draw(sprite, Layer.Foreground1);



        var circle = new Circle() {
            Radius = 10f,
            FillColor = Color4.Transparent,
            StrokeColor = Color4.White,
            StrokeWidth = 1f,
            Origin = new Vector2(0.5f),
            Position = Position
        };

        //Game.Draw(circle, Layer.Foreground1);


    }

    public void SetPosition(Vector2 newPosition, bool interpolate = false) {
        basePosition = newPosition;
    }

    public void SetTangent(float newTangent) => tangent = newTangent;

    public void PlayerAttack(Action<Mouse, Time> attack, Time time) {
        queuedAttacks.Enqueue((attack, time));
    }

    public void Interpolate(Vector2 offset, Time duration) {
        interpolationOffset = offset;
        interpolationTimer = new Timer(duration);
    }




}