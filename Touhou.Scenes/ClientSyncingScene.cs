using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

using Touhou.Objects.Generics;

namespace Touhou.Scenes;

public class ClientSyncingTScene : TScene {

    private const int TOTAL_REQUESTS = 5;
    private const double REQUEST_FREQUENCY = 500; // ms

    private List<(Time RoundTripTime, Time Offset)> timeResponses = new();

    private int requestCount = 0;

    private Clock requestTimer = new();

    private readonly Text text;

    public ClientSyncingTScene() {

        text = new Text {
            DisplayedText = "Syncing...",
            CharacterSize = 40f,
            Origin = Vector2.UnitY,
            IsUI = true,
            Alignment = new Vector2(-1f, 1f),
        };
    }

    public override void OnInitialize() {
        AddEntity(new ReceiveCallback(ReceiveCallback));
        AddEntity(new UpdateCallback(UpdateCallback));
        AddEntity(new RenderCallback(RenderCallback));

        Request();
    }



    private void ReceiveCallback(Packet packet, IPEndPoint endPoint) {
        if (packet.Type == PacketType.TimeResponse && timeResponses.Count < TOTAL_REQUESTS) {

            packet.Out(out Time ourTime).Out(out Time theirTime);

            var roundTripTime = Game.Get<Network>().Time - ourTime;
            var latency = Time.InSeconds(roundTripTime.AsSeconds() / 2f);
            var targetTime = ourTime + latency;

            //Console.ForegroundColor = ConsoleColor.Cyan;
            //Console.WriteLine($"RTR | Estimate: {targetTime}, Actual: {theirTime}, Offset: {theirTime - targetTime}");

            timeResponses.Add((RoundTripTime: roundTripTime, Offset: theirTime - targetTime));

            if (timeResponses.Count == TOTAL_REQUESTS) {

                var averageRTT = (Time)Math.Round(timeResponses.Average(e => e.RoundTripTime));

                // // pick the response closest to the average RTT
                // var averageResponse = timeResponses.MinBy(e => Math.Abs(e.RoundTripTime - averageRTT));
                // Game.Get<Network>().TimeOffset += averageResponse.Offset;

                // pick response with smallest RTT
                var minResponse = timeResponses.MinBy(e => (long)e.RoundTripTime);
                Game.Get<Network>().TimeOffset += minResponse.Offset;

                // var matchStartTime = Game.Get<Network>().Time + Time.InSeconds(3);
                // Game.Get<Network>().Send(new Packet(PacketType.SyncFinished).In(matchStartTime));
                // Game.Get<SceneManager>().ChangeTScene<MatchTScene>(false, false, matchStartTime);

                Game.Get<Network>().Send(new Packet(PacketType.SyncFinished));
                Game.Get<SceneManager>().ChangeScene<CharacterSelecTScene>(false, false);
            }
        }
    }

    private void UpdateCallback() {
        if (requestCount < TOTAL_REQUESTS && requestTimer.Elapsed.AsMilliseconds() > REQUEST_FREQUENCY) {
            Request();
        }
    }

    private void RenderCallback() {
        Game.Get<Renderer>().Queue(text, Layer.UI1);
    }

    private void Request() {
        //Console.ForegroundColor = ConsoleColor.DarkGray;
        //Console.WriteLine($"Requesting Time");
        var packet = new Packet(PacketType.TimeRequest).In(Game.Get<Network>().Time);
        Game.Get<Network>().Send(packet);
        requestCount++;
        requestTimer.Restart();
    }

    public override void OnDisconnect() {
        if (Game.Get<Settings>().UseSteam) Game.Get<Network>().DisconnectSteam();
        else Game.Get<Network>().Disconnect();

        Log.Warn("Opponent disconnected");

        Game.Get<SceneManager>().ChangeScene<MainTScene>();
    }
}
