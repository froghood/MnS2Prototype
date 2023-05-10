// using SFML.System;

// namespace Touhou.Objects;
// public partial class ProjectileManager {

//     private readonly GameManager gameManager;

//     private int totalIds;

//     private List<Projectile> playerProjectiles = new();
//     private List<Projectile> opponentProjectiles = new();

//     private Dictionary<int, Projectile> projectilesById = new();

//     public ProjectileManager(GameManager gameManager) {
//         this.gameManager = gameManager;
//     }

//     public void OnUpdate(Time time, float delta) {
//         foreach (var projectile in this.playerProjectiles) {
//             projectile.Update(time, delta);
//             if (projectile.Destroyed) this.projectilesById.Remove(projectile.Id);

//         }
//         this.playerProjectiles = this.playerProjectiles.Where(e => !e.Destroyed).ToList();

//         foreach (var projectile in this.opponentProjectiles) {
//             projectile.Update(time, delta);
//             if (projectile.Destroyed) this.projectilesById.Remove(projectile.Id);
//         }
//         this.opponentProjectiles = this.opponentProjectiles.Where(e => !e.Destroyed).ToList();
//     }

//     public void OnRender(Time time, float delta) {
//         foreach (var projectile in this.playerProjectiles) {
//             projectile.Render(time, delta);
//         }

//         foreach (var projectile in this.opponentProjectiles) {
//             projectile.Render(time, delta);
//         }
//     }

//     public void OnFinalize(Time time, float delta) {
//         foreach (var projectile in this.playerProjectiles) {
//             projectile.Finalize(time, delta);
//         }

//         foreach (var projectile in this.opponentProjectiles) {
//             projectile.Finalize(time, delta);
//         }
//     }

//     private void AddProjectile(Projectile projectile, bool hasCollision, bool identifiable) {
//         if (identifiable) {
//             projectile.SetId(NextId());
//             this.projectilesById.Add(projectile.Id, projectile);
//         }

//         if (hasCollision) {
//             this.opponentProjectiles.Add(projectile);
//         } else {
//             this.playerProjectiles.Add(projectile);
//         }
//     }

//     private int NextId() {
//         var nextId = totalIds;
//         totalIds++;
//         return nextId;
//     }
// }