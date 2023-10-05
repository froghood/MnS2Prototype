using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class PlayerSakuya : Player {
    public PlayerSakuya(bool isP1) : base(isP1) {

        Speed = 350f;
        FocusedSpeed = 250f;

        AddAttack(PlayerActions.Primary, new SakuyaPrimary());
        AddAttack(PlayerActions.Secondary, new SakuyaSecondary());
        AddAttack(PlayerActions.SpecialA, new SakuyaSpecialA());
        AddAttack(PlayerActions.SpecialB, new SakuyaSpecialB());

        AddBomb(PlayerActions.Bomb, new ReimuBomb());



    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("sakuya") {
            Origin = new Vector2(0.45f, 0.35f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.22f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layers.Player);

        base.Render();
    }
}