using System.Net;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Touhou.Net;

namespace Touhou.Objects;

// public class GameManager {

//     public bool Hosting { get => _hosting; }
//     public long StartTime { get => _startTime; }

//     public Opponent Opponent { get => _opponent; }

//     public ProjectileManager Projectiles { get; private init; }

//     private List<Entity> entities = new();

//     private SpatialPartition<Hitbox> _collisionGrid = new(16, 9);


//     private int _totalPlayerProjectiles;

//     private bool _hosting;
//     private long _startTime;

//     private Player _player;
//     private Opponent _opponent;


//     public GameManager(bool hosting, long startTime) {
//         _hosting = hosting;
//         _startTime = startTime;

//         Projectiles = new ProjectileManager(this);

//         _player = new Player<Character>(this);
//         _opponent = new Opponent(this, new Vector2f(Game.Window.Size.X / (!_hosting ? 3f : 1.5f), Game.Window.Size.Y / 2f), new Color(255, 0, 0));

//     }

//     public void OnReceive(Packet packet, IPEndPoint endPoint) {
//         _opponent.OnReceive(packet, endPoint);
//     }

//     public void OnPress(ActionData action) {
//         _player.OnPress(action);
//     }

//     public void OnRelease(ActionData action) {
//         _player.OnRelease(action);
//     }

//     public void OnUpdate(Time time, float delta) {
//         _player.OnUpdate(time, delta);
//         _opponent.OnUpdate(time, delta);

//         Projectiles.OnUpdate(time, delta);
//     }

//     public void OnRender(Time time, float delta) {
//         _player.OnRender(time, delta);
//         _opponent.OnRender(time, delta);

//         Projectiles.OnRender(time, delta);
//     }

//     public void OnFinalize(Time time, float delta) {
//         Projectiles.OnFinalize(time, delta);
//     }
// }