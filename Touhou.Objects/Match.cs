using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Characters;
using Touhou.Scenes;

namespace Touhou.Objects;

public class Match : Entity, IReceivable {
    private bool isP1;

    public Time StartTime { get; }
    public Time EndTime { get; }

    private Player player;
    private Opponent opponent;
    private Dictionary<PacketType, Action<Packet>> receiveMethods;

    public Time CurrentTimeReal { get => Game.Network.Time - StartTime; }
    public Time CurrentTime { get; private set; }

    public bool Started { get; private set; }

    public Vector2 Bounds { get; private set; } = new(795f, 368f);

    //private Text text = new();

    public List<(Time Time, int Increase)> powerGenerationIncreaseBreakpoints = new() {
        (Time.InSeconds(0f), 8),
        (Time.InSeconds(49f), 8),
        (Time.InSeconds(99f), 16),


    };
    private bool isOpponentDead;
    private Time opponentDeathTime;
    private bool opponentMatchStarted;

    public int TotalPowerGenerated {
        get {
            float powerGen = 0f;
            foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
                powerGen += MathF.Max(MathF.Floor((CurrentTime - breakpoint.Time).AsSeconds() * 5f) / 5f, 0f) * breakpoint.Increase;
            }

            return (int)MathF.Round(powerGen);
        }
    }



    public Match(bool isP1, Time startTime, Player player, Opponent opponent) {
        this.isP1 = isP1;

        StartTime = startTime;
        EndTime = startTime + Time.InSeconds(99f);

        this.player = player;
        this.opponent = opponent;

        this.receiveMethods = new Dictionary<PacketType, Action<Packet>> {
            {PacketType.MatchStarted, MatchStarted},
            {PacketType.Death, Death},
            {PacketType.Rematch, Rematch},
        };
    }



    public override void Update() {
        CurrentTime = Math.Max(CurrentTime, CurrentTimeReal);

        if (!Started && Game.Network.Time >= StartTime) {
            Started = true;

            Game.Network.Send(new Packet(PacketType.MatchStarted));
        }

        if (isOpponentDead && Game.Time - opponentDeathTime >= Time.InSeconds(3f)) {

            var matchStartTime = Game.Network.Time + Time.InSeconds(3f);

            var packet = new Packet(PacketType.Rematch).In(matchStartTime);
            Game.Network.Send(packet);

            Game.Command(() => {
                Game.Scenes.ChangeScene<MatchScene>(false, isP1, matchStartTime, player.GetType(), opponent.GetType());
            });
        }
    }





    public int GetPowerPerSecond() {
        int powerPerSecond = 0;
        foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
            if (CurrentTime >= breakpoint.Time) powerPerSecond += breakpoint.Increase;
        }
        return powerPerSecond;
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.MatchStarted && !opponentMatchStarted) return;

        if (receiveMethods.TryGetValue(packet.Type, out var method)) {
            method?.Invoke(packet);
        }
    }

    private void Death(Packet packet) {
        packet.Out(out Time theirTime, true);


        // win if:
        // player is not dead,
        // player died after opponent,
        // player and opponent died at the same time but player is P1
        bool playerWins = (
            !player.IsDead ||
            player.DeathTime > theirTime ||
            player.DeathTime == theirTime && isP1);

        if (playerWins) {

            isOpponentDead = true;
            opponentDeathTime = Game.Time;

        }

    }

    private void Rematch(Packet packet) {

        packet.Out(out Time matchStartTime);

        Game.Command(() => {
            Game.Scenes.ChangeScene<MatchScene>(false, isP1, matchStartTime, player.GetType(), opponent.GetType());
        });
    }

    private void MatchStarted(Packet packet) => opponentMatchStarted = true;
}