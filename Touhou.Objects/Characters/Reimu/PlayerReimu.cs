using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class PlayerReimu : Player {

    public PlayerReimu(bool isP1) : base(isP1) {

        Speed = 300f;
        FocusedSpeed = 150f;

        AddAttack(PlayerActions.Primary, new ReimuPrimary());
        AddAttack(PlayerActions.Secondary, new ReimuSecondary());
        AddAttack(PlayerActions.SpecialA, new ReimuSpecialA());
        AddAttack(PlayerActions.SpecialB, new ReimuSpecialB());

        AddBomb(PlayerActions.Bomb, new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("reimu") {
            Origin = new Vector2(0.42f, 0.28f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Opponent.Position.X), 1f) * 0.2f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layer.Player);

        base.Render();
    }
}