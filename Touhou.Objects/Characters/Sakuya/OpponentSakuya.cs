using OpenTK.Mathematics;
using Touhou.Graphics;

namespace Touhou.Objects.Characters;

public class OpponentSakuya : Opponent {
    public OpponentSakuya(bool isP1) : base(isP1) {

        AddAttack(PlayerActions.Primary, new SakuyaPrimary());
        AddAttack(PlayerActions.Secondary, new SakuyaSecondary());
        AddAttack(PlayerActions.SpecialA, new SakuyaSpecialA());
        AddAttack(PlayerActions.SpecialB, new SakuyaSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("sakuya") {
            Origin = new Vector2(0.45f, 0.35f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Player.Position.X), 1f) * 0.22f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layers.Opponent);

        base.Render();
    }
}