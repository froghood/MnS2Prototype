using System.Net;
using OpenTK.Mathematics;
using Touhou.Networking;
using Touhou.Objects;
using Touhou.Objects.Characters;
using Touhou.Scenes;

namespace Touhou.Objects;

public class MatchOld : Entity, IReceivable {

    public Time StartTime { get => startTime; }
    public Time EndTime { get => endTime; }

    public Time CurrentTimeReal { get => Game.NetworkOld.Time - StartTime; }
    public Time CurrentTime { get; private set; }

    public bool HasStarted { get => hasStarted; }

    public Vector2 Bounds { get => new Vector2(795f, 368f); }

    private readonly bool isP1;
    private readonly Time startTime;
    private readonly CharacterOption localOption;
    private readonly CharacterOption remoteOption;
    private readonly Time endTime;

    private readonly Character localCharacter;
    private readonly Character remoteCharacter;
    private readonly Dictionary<PacketType, Action<Packet>> receiveMethods;



    //private Text text = new();

    public List<(Time Time, int Increase)> powerGenerationIncreaseBreakpoints = new() {
        (Time.InSeconds(0f), 8),
        (Time.InSeconds(49f), 8),
        (Time.InSeconds(99f), 16),


    };
    private bool isRemoteDead;
    private Time remoteDeathTime;
    private bool hasRemoteMatchStarted;
    private bool hasStarted;

    public int TotalPowerGenerated {
        get {
            float powerGen = 0f;
            foreach (var breakpoint in powerGenerationIncreaseBreakpoints) {
                powerGen += MathF.Max(MathF.Floor((CurrentTime - breakpoint.Time).AsSeconds() * 5f) / 5f, 0f) * breakpoint.Increase;
            }

            return (int)MathF.Round(powerGen);
        }
    }



    public MatchOld(bool isP1, Time startTime, CharacterOption localOption, CharacterOption remoteOption, Character localCharacter, Character remoteCharacter) {

        this.isP1 = isP1;
        this.startTime = startTime;
        this.localOption = localOption;
        this.remoteOption = remoteOption;

        this.endTime = startTime + Time.InSeconds(99f);

        this.localCharacter = localCharacter;
        this.remoteCharacter = remoteCharacter;

        this.receiveMethods = new Dictionary<PacketType, Action<Packet>> {
            {PacketType.MatchStarted, MatchStarted},
            {PacketType.Death, Death},
            {PacketType.Rematch, Rematch},
        };
    }



    public override void Update() {
        CurrentTime = Math.Max(CurrentTime, CurrentTimeReal);

        if (!hasStarted && Game.NetworkOld.Time >= startTime) {
            hasStarted = true;

            Game.NetworkOld.Send(PacketType.MatchStarted);
        }

        if (isRemoteDead && Game.Time - remoteDeathTime >= Time.InSeconds(3f)) {

            var matchStartTime = Game.NetworkOld.Time + Time.InSeconds(3f);

            Game.NetworkOld.Send(PacketType.Rematch, matchStartTime);

            Game.Command(() => {
                Game.Scenes.ChangeScene<NetplayMatchScene>(false, isP1, matchStartTime, localOption, remoteOption);
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

    public void Receive(Packet packet) {
        if (packet.Type != PacketType.MatchStarted && !hasRemoteMatchStarted) return;

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
        bool localWins = (
            localCharacter.State != CharacterState.Dead ||
            localCharacter.DeathTime > theirTime ||
            localCharacter.DeathTime == theirTime && isP1);

        if (localWins) {

            isRemoteDead = true;
            remoteDeathTime = Game.Time;

        }

    }

    private void Rematch(Packet packet) {

        packet.Out(out Time matchStartTime);

        Game.Command(() => {
            Game.Scenes.ChangeScene<NetplayMatchScene>(false, isP1, matchStartTime, localOption, remoteOption);
        });
    }

    private void MatchStarted(Packet packet) => hasRemoteMatchStarted = true;
}