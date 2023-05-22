using System.Net;
using SFML.Graphics;
using Touhou.Net;

namespace Touhou.Objects;

public abstract class Projectile : Entity, IReceivable {

    public static uint totalLocalProjectiles;
    public static uint totalRemoteProjectiles = 0x80000000;



    public bool IsRemote { get; }

    public Time SpawnTime { get; }

    public uint Id { get; }

    public bool DestroyedOnScreenExit { get; set; } = true;

    public Color Color { get; set; } = Color.White;

    public bool Grazed { get; private set; }

    public int GrazeAmount { get; set; }



    public Projectile(bool isRemote, Time spawnTimeOffset = default(Time)) {
        IsRemote = isRemote;
        SpawnTime = Game.Time - spawnTimeOffset;

        Id = IsRemote ? totalRemoteProjectiles++ : totalLocalProjectiles++;
    }



    public override void Update() {
        if (DestroyedOnScreenExit) {
            foreach (var hitbox in Hitboxes) {
                var bounds = hitbox.GetBounds();
                if (!(bounds.Left >= Game.Window.Size.X || bounds.Left + bounds.Width <= 0 ||
                bounds.Top >= Game.Window.Size.Y || bounds.Top + bounds.Height <= 0)) {
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



    public void Graze() => Grazed = true;
}