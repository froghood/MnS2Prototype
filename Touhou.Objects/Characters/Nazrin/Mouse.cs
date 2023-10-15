using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class Mouse : Entity {

    public float Tangent { get => tangent; }
    private float tangent;
    private Queue<(Action<Mouse, Time> Attack, Time Time)> queuedAttacks = new();


    public override void Update() {

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
        Position = newPosition;
    }

    public void SetTangent(float newTangent) => tangent = newTangent;

    public void PlayerAttack(Action<Mouse, Time> attack, Time time) {
        queuedAttacks.Enqueue((attack, time));
    }




}