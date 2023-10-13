using OpenTK.Mathematics;
using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class PlayerNazrin : Player {

    private LinkedList<Mouse> mice = new();


    public PlayerNazrin(bool isP1) : base(isP1) {

        Speed = 325f;
        FocusedSpeed = 125f;

        AddAttack(PlayerActions.Primary, new NazrinPrimary());
        AddAttack(PlayerActions.Secondary, new NazrinSecondary());
        AddAttack(PlayerActions.SpecialA, new NazrinSpecialA());
        AddAttack(PlayerActions.SpecialB, new NazrinSpecialB());

        AddBomb(new ReimuBomb());
    }

    public override void Render() {

        if (IsDead) return;

        base.Render();
    }


    public void AddMouse() {
        Scene.AddEntity(new Mouse(() => Position, mice));
    }

    protected override void ChangeVelocity(Vector2 newVelocity) {
        if (newVelocity == Velocity) return;
        Velocity = newVelocity;

        var angles = mice.Select(e => e.CompressedLineAngle).ToArray();

        foreach (var mouse in mice) {
            mouse.RecaluclateSmoothPosition(Position, mouse.CompressedLineAngle / 256f * MathF.Tau);
        }

        Log.Info($"mice angles: {string.Join(", ", angles)}");

        var packet = new Packet(PacketType.VelocityChanged)
        .In(Game.Network.Time)
        .In(Position)
        .In(Velocity)
        .In(Focused)
        .In(angles);

        Game.Network.Send(packet);
    }

}