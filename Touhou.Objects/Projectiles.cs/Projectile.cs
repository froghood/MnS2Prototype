using System.Net;
using OpenTK.Mathematics;
using Touhou.Graphics;
using Touhou.Networking;

namespace Touhou.Objects.Projectiles;

public abstract class Projectile : Entity, IReceivable {

    public enum ProjectileType {
        Unknown = 0,
        Amulet,
        LocalHoming,
        RemoteHoming,
        LocalTargetingAmuletGroup,
        RemoteTargetingAmuletGroup,
        TargetingAmulet,
        SpecialAmulet,
        YinYang,
        BombWave,
    }

    public static uint TotalLocalProjectiles;
    public static uint TotalRemoteProjectiles = 0x80000000;


    public bool IsPlayerOwned { get; }
    public bool IsRemote { get; }

    public uint Id { get; }

    public bool DestroyedOnScreenExit { get; set; } = true;

    public Color4 Color { get; set; } = Color4.White;

    public bool Grazed { get; private set; }

    public int GrazeAmount { get; set; }

    protected Match Match => match is null ? match = Scene.GetFirstEntity<Match>() : match;
    private Match match;

    private static int lastLocalFrame = 0;
    private static int lastRemoteFrame = 0;
    private static List<(uint, ProjectileType, bool, bool, Color4)> currentLocalProjectileGroup;
    private static List<(uint, ProjectileType, bool, bool, Color4)> currentRemoteProjectileGroup;
    public static Queue<List<(uint, ProjectileType, bool, bool, Color4)>> LocalProjectileHistory = new();
    public static Queue<List<(uint, ProjectileType, bool, bool, Color4)>> RemoteProjectileHistory = new();

    public Projectile(bool isPlayerOwned, bool isRemote) {
        IsPlayerOwned = isPlayerOwned;
        IsRemote = isRemote;

        Id = IsRemote ? TotalRemoteProjectiles++ : TotalLocalProjectiles++;






    }

    public override void Init() {

        var type = this switch {
            SpecialAmulet => ProjectileType.SpecialAmulet,
            Amulet => ProjectileType.Amulet,
            LocalHomingAmulet => ProjectileType.LocalHoming,
            RemoteHomingAmulet => ProjectileType.RemoteHoming,
            LocalTargetingAmuletGroup => ProjectileType.LocalTargetingAmuletGroup,
            RemoteTargetingAmuletGroup => ProjectileType.RemoteTargetingAmuletGroup,
            TargetingAmulet => ProjectileType.TargetingAmulet,
            YinYang => ProjectileType.YinYang,
            ReimuBombWave => ProjectileType.BombWave,
            _ => ProjectileType.Unknown,
        };

        if (IsRemote) {

            if (lastRemoteFrame != Game.FrameCount) {
                currentRemoteProjectileGroup = new List<(uint, ProjectileType, bool, bool, Color4)>();
                RemoteProjectileHistory.Enqueue(currentRemoteProjectileGroup);

                while (RemoteProjectileHistory.Count > 15 && RemoteProjectileHistory.TryDequeue(out _)) ;
            }

            currentRemoteProjectileGroup.Add((Id ^ 0x80000000, type, IsPlayerOwned, IsRemote, Color));
            lastRemoteFrame = Game.FrameCount;

            // if (lastRemoteFrame != Game.FrameCount) Game.Log("remoteprojectiles", "");
            // Game.Log("remoteprojectiles", $"{Id ^ 0x80000000}: {this.GetType().Name}");
            // lastRemoteFrame = Game.FrameCount;
        } else {

            if (lastLocalFrame != Game.FrameCount) {
                currentLocalProjectileGroup = new List<(uint, ProjectileType, bool, bool, Color4)>();
                LocalProjectileHistory.Enqueue(currentLocalProjectileGroup);

                while (LocalProjectileHistory.Count > 15 && LocalProjectileHistory.TryDequeue(out _)) ;

            }

            currentLocalProjectileGroup.Add((Id, type, IsPlayerOwned, IsRemote, Color));
            lastLocalFrame = Game.FrameCount;
            // if (lastLocalFrame != Game.FrameCount) Game.Log("localprojectiles", "");
            // Game.Log("localprojectiles", $"{Id}: {this.GetType().Name}");
            // lastLocalFrame = Game.FrameCount;
        }

        base.Init();
    }



    public override void Update() {
        DestroyIfOutOfBounds();
    }

    private void DestroyIfOutOfBounds() {
        if (Hitboxes.Count == 0) return;
        if (DestroyedOnScreenExit && Game.Time >= CreationTime + Time.InSeconds(1f)) {
            foreach (var hitbox in Hitboxes) {
                var bounds = hitbox.GetBounds();
                if ((bounds.Min.X < Match.Bounds.X && bounds.Max.X > -Match.Bounds.X) &&
                    (bounds.Min.Y < Match.Bounds.Y && bounds.Max.Y > -Match.Bounds.Y)) {
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

    public virtual void NetworkDestroy() {
        Destroy();

        var packet = new Packet(PacketType.DestroyProjectile).In(Id ^ 0x80000000);
        Game.Network.Send(packet);
    }
}