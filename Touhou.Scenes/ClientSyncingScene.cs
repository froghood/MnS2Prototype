using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

using Touhou.Objects.Generics;

namespace Touhou.Scenes;
public class ClientSyncingScene : Scene {

    private const int TOTAL_REQUESTS = 5;
    private const double REQUEST_FREQUENCY = 500; // ms

    private List<(Time RoundTripTime, Time Offset)> timeResponses = new();

    private int requestCount = 0;

    private Clock requestTimer = new();

    private readonly Text text;

    public ClientSyncingScene() {

        text = new Text {
            DisplayedText = "Syncing...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {
        AddEntity(new Objects.Generics.ReceiveCallback(ReceiveCallback));
        AddEntity(new UpdateCallback(UpdateCallback));
        AddEntity(new RenderCallback(RenderCallback));

        Request();
    }



    private void ReceiveCallback(Packet packet) {
        if (packet.Type == PacketType.TimeResponse && timeResponses.Count < TOTAL_REQUESTS) {

            packet.Out(out Time ourTime).Out(out Time theirTime);

            var roundTripTime = Game.NetworkOld.Time - ourTime;
            var latency = Time.InSeconds(roundTripTime.AsSeconds() / 2f);
            var targetTime = ourTime + latency;

            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine($"RTR | Estimate: {targetTime}, Actual: {theirTime}, Offset: {theirTime - targetTime}");

            timeResponses.Add((RoundTripTime: roundTripTime, Offset: theirTime - targetTime));

            if (timeResponses.Count == TOTAL_REQUESTS) {

                var averageRTT = (Time)Math.Round(timeResponses.Average(e => e.RoundTripTime));

                // // pick the response closest to the average RTT
                // var averageResponse = timeResponses.MinBy(e => Math.Abs(e.RoundTripTime - averageRTT));
                // Game.Network.TimeOffset += averageResponse.Offset;

                // pick response with smallest RTT
                var minResponse = timeResponses.MinBy(e => (long)e.RoundTripTime);
                Game.NetworkOld.TimeOffset += minResponse.Offset;

                // var matchStartTime = Game.Network.Time + Time.InSeconds(3);
                // Game.Network.Send(new Packet(PacketType.SyncFinished),matchStartTime);
                // Game.Scenes.ChangeScene<MatchScene>(false, false, matchStartTime);

                Game.NetworkOld.Send(PacketType.SyncFinished);
                Game.Scenes.ChangeScene<CharacterSelectScene>(false, false);
            }
        }
    }

    private void UpdateCallback() {
        if (requestCount < TOTAL_REQUESTS && requestTimer.Elapsed.AsMilliseconds() > REQUEST_FREQUENCY) {
            Request();
        }
    }

    private void RenderCallback() {
        Game.Draw(text, Layer.UI1);
    }

    private void Request() {
        //Console.ForegroundColor = ConsoleColor.DarkGray;
        //Console.WriteLine($"Requesting Time");
        Game.NetworkOld.Send(PacketType.TimeRequest, Game.NetworkOld.Time);

        requestCount++;
        requestTimer.Restart();
    }

    public override void OnDisconnect() {
        if (Game.Settings.UseSteam) Game.NetworkOld.DisconnectSteam();
        else Game.NetworkOld.Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Scenes.ChangeScene<MainScene>();
    }
}
