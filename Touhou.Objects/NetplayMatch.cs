using System.Net;
using Touhou.Networking;
using Touhou.Objects.Characters;
using Touhou.Scenes;

namespace Touhou.Objects;

public class NetplayMatch : Match, IReceivable {



    public override Time CurrentTime { get => currentTime = Time.Max(currentTime, Game.NetworkOld.Time - StartTime); protected set => currentTime = value; }
    private Time currentTime;


    private readonly CharacterOption localOption;
    private readonly CharacterOption remoteOption;
    private readonly Character localCharacter;
    private readonly Character remoteCharacter;
    private bool localWins;
    private Timer rematchTimer;
    private bool hasRemoteMatchStarted;
    private bool previousRemoteIsDead;
    private bool hasMatchStartedPacketBeenSent;

    public NetplayMatch(bool isP1, Time startTime, CharacterOption localOption, CharacterOption remoteOption, Character localCharacter, Character remoteCharacter) : base(isP1, startTime) {

        this.localOption = localOption;
        this.remoteOption = remoteOption;
        this.localCharacter = localCharacter;
        this.remoteCharacter = remoteCharacter;

        currentTime = Game.NetworkOld.Time - startTime;


    }

    public override void Update() {

        if (!hasMatchStartedPacketBeenSent && HasStarted) {
            Game.NetworkOld.Send(PacketType.MatchStarted);
            hasMatchStartedPacketBeenSent = true;
        }

        CheckForRemoteDeath();

        if (localWins && rematchTimer.HasFinished) {
            LocalRematch();
        }
    }

    private void CheckForRemoteDeath() {
        if (!previousRemoteIsDead && remoteCharacter.State == CharacterState.Dead) {
            previousRemoteIsDead = true;

            RemoteDeath();
        }
    }

    private void LocalRematch() {

        var matchStartTime = Game.NetworkOld.Time + Time.InSeconds(3f);

        Game.NetworkOld.Send(PacketType.Rematch, matchStartTime);

        Game.Command(() => {
            Game.Scenes.ChangeScene<NetplayMatchScene>(false, IsP1, matchStartTime, localOption, remoteOption);
        });

    }

    private void RemoteDeath() {
        localCharacter.ApplyInvulnerability();

        localWins = (
            localCharacter.State != CharacterState.Dead ||
            localCharacter.DeathTime > remoteCharacter.DeathTime ||
            localCharacter.DeathTime == remoteCharacter.DeathTime && localCharacter.IsP1
        );

        if (localWins) {
            rematchTimer = new Timer(Time.InSeconds(3f));
        }
    }

    private void RemoteMatchStarted(Packet packet) => hasRemoteMatchStarted = true;


    private void RemoteRematch(Packet packet) {
        packet.Out(out Time matchStartTime);

        Game.Command(() => {
            Game.Scenes.ChangeScene<NetplayMatchScene>(false, IsP1, matchStartTime, localOption, remoteOption);
        });
    }



    public void Receive(Packet packet) {
        if (packet.Type != PacketType.MatchStarted && !hasRemoteMatchStarted) return;

        switch (packet.Type) {
            case PacketType.MatchStarted:
                RemoteMatchStarted(packet);
                break;

            case PacketType.Rematch:
                RemoteRematch(packet);
                break;
        }


    }


}