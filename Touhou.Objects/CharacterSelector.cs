using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;
using Touhou.Objects.Characters;
using Touhou.Scenes;

namespace Touhou.Objects;

public class CharacterSelector : Entity, IControllable, IReceivable {
    private bool isP1;
    private CharacterOption[] characterOptions;

    private int localIndex;
    private int remoteIndex;

    private CharacterOption localOption { get => ((CharacterOption)localIndex); }
    private CharacterOption remoteOption { get => ((CharacterOption)remoteIndex); }
    private bool playerSelected;
    private bool opponentSelected;
    private Dictionary<PacketType, Action<Packet>> receiveCallbacks;

    public CharacterSelector(bool isP1) {

        this.isP1 = isP1;

        characterOptions = Enum.GetValues<CharacterOption>();

        receiveCallbacks = new Dictionary<PacketType, Action<Packet>> {
            {PacketType.ChangedCharacter, ChangedCharacter},
            {PacketType.SelectedCharacter, SelectedCharacter},
            {PacketType.DeselectedCharacter, DeselectedCharacter},
            {PacketType.MatchReady, MatchReady}
        };
    }



    public void Press(PlayerActions action) {



        if (!playerSelected) {
            int prevPosition = localIndex;
            localIndex += action switch {
                PlayerActions.Up => -1,
                PlayerActions.Down => 1,
                _ => 0
            };

            localIndex = Math.Clamp(localIndex, 0, characterOptions.Length - 1);

            if (localIndex != prevPosition) {
                var packet = new Packet(PacketType.ChangedCharacter).In(localIndex - prevPosition);
                Game.Network.Send(packet);
            }
        }



        if (isP1 && playerSelected && opponentSelected && action == PlayerActions.Primary) {


            var matchStartTime = Game.Network.Time + Time.InSeconds(3f);

            var packet = new Packet(PacketType.MatchReady)
            .In(matchStartTime)
            .In(localOption)
            .In(remoteOption);

            Game.Network.Send(packet);

            Game.Command(() => {
                Game.Scenes.ChangeScene<NetplayMatchScene>(false, true, matchStartTime, localOption, remoteOption);
            });

        }


        if (!playerSelected && action == PlayerActions.Primary) {

            playerSelected = true;

            var packet = new Packet(PacketType.SelectedCharacter);
            Game.Network.Send(packet);

        }

        if (playerSelected && action == PlayerActions.Secondary) {

            playerSelected = false;

            var packet = new Packet(PacketType.DeselectedCharacter);
            Game.Network.Send(packet);
        }

    }

    public void Release(PlayerActions action) { }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        if (receiveCallbacks.TryGetValue(packet.Type, out var callback)) {
            callback.Invoke(packet);
        }
    }

    public override void Render() {
        for (int i = 0; i < characterOptions.Length; i++) {
            var option = characterOptions[i];

            var text = new Text() {
                Origin = new Vector2(0.5f, 0.3f),
                Position = Vector2.UnitY * -100f * i,
                DisplayedText = option.ToString(),
                CharacterSize = 80f,
                Font = "consolas",
                IsUI = true
            };

            Game.Draw(text, Layer.UI1);
        }

        var playerText = new Text() {
            Origin = new Vector2(isP1 ? 1f : 0f, 0.3f),
            Position = new Vector2(
                isP1 ? -300f : 300f,
                -100f * localIndex
            ),
            Color = new Color4(0f, 1f, 0f, 1f),
            DisplayedText = isP1 ? "P1" : "P2",
            CharacterSize = 60f,
            Font = "consolas",
            IsUI = true
        };

        var opponentText = new Text() {
            Origin = new Vector2(!isP1 ? 1f : 0f, 0.3f),
            Position = new Vector2(
                !isP1 ? -300f : 300f,
                -100f * remoteIndex
            ),
            Color = new Color4(1f, 0f, 0f, 1f),
            DisplayedText = !isP1 ? "P1" : "P2",
            CharacterSize = 60f,
            Font = "consolas",
            IsUI = true
        };

        Game.Draw(playerText, Layer.UI1);
        Game.Draw(opponentText, Layer.UI1);

        if (playerSelected) {
            var selectedFade = new Sprite("fade") {
                Origin = new Vector2(0f, 0.5f),
                Position = new Vector2(
                    isP1 ? -300f : 300f,
                    -100f * localIndex
                ),
                Scale = new Vector2(6.5f, 0.8f) * new Vector2(isP1 ? 1 : -1f, 1f),
                Color = new Color4(0f, 1f, 0f, 1f),
                IsUI = true,
                BlendMode = BlendMode.Additive
            };

            Game.Draw(selectedFade, Layer.UI1);
        }

        if (opponentSelected) {
            var selectedFade = new Sprite("fade") {
                Origin = new Vector2(0f, 0.5f),
                Position = new Vector2(
                    !isP1 ? -300f : 300f,
                    -100f * remoteIndex
                ),
                Scale = new Vector2(6.5f, 0.8f) * new Vector2(!isP1 ? 1 : -1f, 1f),
                Color = new Color4(1f, 0f, 0f, 1f),
                IsUI = true,
                BlendMode = BlendMode.Additive
            };

            Game.Draw(selectedFade, Layer.UI1);
        }

    }

    private void ChangedCharacter(Packet packet) {
        packet.Out(out int direction, true);

        remoteIndex += direction;
    }

    private void SelectedCharacter(Packet packet) {
        opponentSelected = true;
    }
    private void DeselectedCharacter(Packet packet) {
        opponentSelected = false;
    }

    private void MatchReady(Packet packet) {
        packet
        .Out(out Time matchStartTime)
        .Out(out CharacterOption remoteOption)
        .Out(out CharacterOption localOption);

        Game.Command(() => {
            Game.Scenes.ChangeScene<NetplayMatchScene>(false, false, matchStartTime, localOption, remoteOption);

        });


    }
}