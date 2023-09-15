using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Net;

namespace Touhou.Objects.Projectiles;

public abstract class Projectile : Entity, IReceivable {

    public static uint totalLocalProjectiles;
    public static uint totalRemoteProjectiles = 0x80000000;


    public bool IsPlayerOwned { get; }
    public bool IsRemote { get; }

    public Time SpawnTime { get; }

    public uint Id { get; }

    public bool DestroyedOnScreenExit { get; set; } = true;

    public Color4 Color { get; set; } = Color4.White;

    public bool Grazed { get; private set; }

    public int GrazeAmount { get; set; }

    protected Match Match => match is null ? match = Scene.GetFirstEntity<Match>() : match;
    private Match match;



    public Projectile(bool isPlayerOwned, bool isRemote, Time spawnTimeOffset = default(Time)) {
        IsPlayerOwned = isPlayerOwned;
        IsRemote = isRemote;
        SpawnTime = Game.Time - spawnTimeOffset;

        Id = IsRemote ? totalRemoteProjectiles++ : totalLocalProjectiles++;
    }



    public override void Update() {
        if (DestroyedOnScreenExit && Game.Time >= SpawnTime + Time.InSeconds(1f)) {
            foreach (var hitbox in Hitboxes) {
                var bounds = hitbox.GetBounds();
                if (!(bounds.Min.X >= Match.Bounds.X || bounds.Max.X <= -Match.Bounds.X ||
                      bounds.Min.Y >= Match.Bounds.Y || bounds.Max.Y <= -Match.Bounds.Y)) {
                    return;
                }
            }
            Destroy();
        }
    }



    public virtual void Receive(Packet packet, IPEndPoint endPoint) {
        if (packet.Type != PacketType.DestroyProjectile) return;

        packet.Out(out uint id, true);

        if (id == Id) Destroy();
    }

    public override void Render() {
        // var idText = new Text() {
        //     DisplayedText = Id.ToString(),
        //     CharacterSize = 12f,
        //     Font = "consolas",
        //     Color = Color4.White,
        //     Position = Position,
        // };

        // Game.Draw(idText);
    }



    public void Graze() => Grazed = true;
}