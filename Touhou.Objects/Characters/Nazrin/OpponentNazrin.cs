using Touhou.Networking;

namespace Touhou.Objects.Characters;

public class OpponentNazrin : Opponent {

    private LinkedList<Mouse> mice = new();

    public OpponentNazrin(bool isP1) : base(isP1) {

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
        Scene.AddEntity(new Mouse(() => basePosition + predictedOffset, mice));
    }

    protected override void VelocityChanged(Packet packet) {
        base.VelocityChanged(packet);

        var angles = new Queue<byte>();

        Log.Info($"remaining: {packet.Remaining}");

        while (packet.Remaining > 0) {
            packet.Out(out byte angle);
            angles.Enqueue(angle);
        }

        //Log.Info($"mice angles: {string.Join(", ", angles)}");


        foreach (var mouse in mice) {
            mouse.RecaluclateSmoothPosition(basePosition, angles.Dequeue() / 256f * MathF.Tau);
        }
    }
}