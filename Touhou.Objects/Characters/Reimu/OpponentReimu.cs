using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects;

namespace Touhou.Objects.Characters;

public class OpponentReimu : Opponent {

    public OpponentReimu(bool isP1) : base(isP1) {

        AddAttack(PlayerActions.Primary, new ReimuPrimary());
        AddAttack(PlayerActions.Secondary, new ReimuSecondary());
        AddAttack(PlayerActions.SpecialA, new ReimuSpecialA());
        AddAttack(PlayerActions.SpecialB, new ReimuSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        var sprite = new Sprite("reimu") {
            Origin = new Vector2(0.45f, 0.35f),
            Position = Position,
            Scale = new Vector2(MathF.Sign(Position.X - Player.Position.X), 1f) * 0.2f,
            Color = Color,
            UseColorSwapping = false,
        };

        Game.Draw(sprite, Layers.Opponent);

        base.Render();
    }
}