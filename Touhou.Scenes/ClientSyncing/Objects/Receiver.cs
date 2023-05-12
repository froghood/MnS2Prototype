using System.Net;
using SFML.Graphics;
using SFML.System;
using Touhou.Net;
using Touhou.Objects;
using Touhou.Scenes.Match;

namespace Touhou.Scenes.ClientSyncing.Objects;

public class Receiver : Entity, IReceivable {

    private const int TOTAL_REQUESTS = 5;
    private const int REQUEST_FREQUENCY = 500; // ms

    private List<(Time RoundTripTime, Time Offset)> timeResponses = new();

    private int requestCount = 0;

    private Clock requestTimer = new();

    private readonly Text text;


    public Receiver() {
        this.text = new Text("Syncing...", Game.DefaultFont, 14);
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type == PacketType.TimeResponse && timeResponses.Count < TOTAL_REQUESTS) {

            packet.Out(out Time ourTime).Out(out Time theirTime);

            var roundTripTime = Game.Network.Time - ourTime;
            var latency = Time.InSeconds(roundTripTime.AsSeconds() / 2f);
            var targetTime = ourTime + latency;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"RTR | Estimate: {targetTime}, Actual: {theirTime}, Offset: {theirTime - targetTime}");

            timeResponses.Add((RoundTripTime: roundTripTime, Offset: theirTime - targetTime));

            if (timeResponses.Count == TOTAL_REQUESTS) {

                var averageRTT = (Time)Math.Round(timeResponses.Average(e => e.RoundTripTime));

                // // pick the response closest to the average RTT
                // var averageResponse = timeResponses.MinBy(e => Math.Abs(e.RoundTripTime - averageRTT));
                // Game.Network.TimeOffset += averageResponse.Offset;

                // pick response with smallest RTT
                var minResponse = timeResponses.MinBy(e => (long)e.RoundTripTime);
                Game.Network.TimeOffset += minResponse.Offset;

                var gameStartTime = Game.Network.Time + Time.InSeconds(4);
                Game.Network.Send(new Packet(PacketType.SyncFinished).In(gameStartTime));
                Game.Scenes.PushScene<MatchScene>(false, gameStartTime);
            }
        }
    }

    public override void Update() {
        if (requestCount < TOTAL_REQUESTS && requestTimer.ElapsedTime.AsMilliseconds() > REQUEST_FREQUENCY) {
            Request();
        }
    }

    public override void Render() {
        Game.Window.Draw(this.text);
    }

    public override void PostRender() { }

    public void Request() {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Requesting Time");
        var packet = new Packet(PacketType.TimeRequest).In(Game.Network.Time);
        Game.Network.Send(packet);
        requestCount++;
        requestTimer.Restart();
    }
}