using System.Net;
using SFML.Window;
using Touhou.Net;
using Touhou.Objects;

namespace Touhou.Scenes;

public abstract class Scene {

    private List<Entity> entities = new();
    private CollisionGrid collisionGrid = new(16, 9);


    public void AddEntity(Entity entity) {
        entity.Scene = this;
        entity.Destroyed += () => {

        };
        entities.Add(entity);

    }

    public IEnumerable<T> GetEntities<T>() where T : Entity {
        for (int index = 0; index < entities.Count; index++) {
            if (entities[index] is T entity) yield return entity;
        }
    }

    public T GetFirstEntity<T>() where T : Entity {
        for (int index = 0; index < entities.Count; index++) {
            if (entities[index] is T entity) return entity;
        }

        return null;
    }

    public void Press(PlayerAction action) {
        IterateEntites(e => {
            if (e is IControllable c) c.Press(action);
        });
    }

    public void Release(PlayerAction action) {
        IterateEntites(e => {
            if (e is IControllable c) c.Release(action);
        });
    }

    public void Receive(Packet packet, IPEndPoint endPoint) {
        IterateEntites(e => {
            if (e is IReceivable o) o.Receive(packet, endPoint);
        });
    }

    public void Update() {
        int numCollisions = 0;

        IterateEntites(entity => {
            entity.Update();
            CheckCollisions(entity, ref numCollisions);
        });

        //System.Console.WriteLine(numCollisions);


    }

    private void CheckCollisions(Entity entity, ref int numCollisions) {
        if (!entity.CanCollide || entity.IsDestroyed) return;

        //var collidedHitboxes = new HashSet<Hitbox>();

        foreach (var hitbox in entity.Hitboxes) {
            var checkedHitboxes = new HashSet<Hitbox>();

            var bounds = hitbox.GetBounds();
            int regionX = (int)MathF.Floor(bounds.Left / collisionGrid.CellWidth);
            int regionY = (int)MathF.Floor(bounds.Top / collisionGrid.CellHeight);
            int regionWidth = (int)MathF.Ceiling((bounds.Left + bounds.Width) / collisionGrid.CellWidth);
            int regionHeight = (int)MathF.Ceiling((bounds.Top + bounds.Height) / collisionGrid.CellHeight);

            for (int x = regionX; x < regionWidth; x++) {
                for (int y = regionY; y < regionHeight; y++) {

                    if (entity.IsDestroyed) return;

                    if (collisionGrid.TryGet(x, y, out var otherHitboxes)) {
                        foreach (var otherHitbox in otherHitboxes) {

                            // ignore checked hitboxes
                            if (checkedHitboxes.Contains(otherHitbox)) continue;
                            checkedHitboxes.Add(otherHitbox);

                            // ignore hitboxes of destroyed entites
                            if (otherHitbox.Entity.IsDestroyed) continue;

                            // ignore collisions between entities own hitboxes
                            if (otherHitbox.Entity == entity) continue;

                            // ignore collisions between entities that share collision groups
                            if (SharesCollisionGroup(entity, otherHitbox.Entity)) continue;

                            bool collided = ((Func<bool>)(otherHitbox switch {
                                PointHitbox point => () => hitbox.Check(point),
                                CircleHitbox circle => () => hitbox.Check(circle),
                                RectangleHitbox rectangle => () => hitbox.Check(rectangle),
                                _ => throw new Exception()
                            })).Invoke();

                            if (collided) {
                                hitbox.Collide(otherHitbox.Entity);
                                otherHitbox.Collide(entity);
                                numCollisions++;
                            }
                        }
                    }
                    collisionGrid.Add(hitbox, x, y);
                }
            }
        }
    }

    private static bool SharesCollisionGroup(Entity entity, Entity other) {
        foreach (int group in entity.CollisionGroups) {
            if (other.CollisionGroups.Contains(group)) return true;
        }
        return false;
    }

    public void Render() {

        IterateEntites(e => e.Render());
    }

    public void DebugRender() {
        collisionGrid.Render();
        IterateEntites(e => e.DebugRender());
    }

    public void PostRender() {
        IterateEntites(e => e.PostRender());
        collisionGrid.Clear();
    }

    private void IterateEntites(Action<Entity> callback) {
        int index = 0;
        while (index < entities.Count) {
            var entity = entities[index];

            if (entity.IsDestroyed) {
                entities.RemoveAt(index);
            } else {
                callback.Invoke(entity);
                index++;
            }
        }
    }

    public virtual void OnInitialize() { }
    public virtual void OnDeactivate() { }
    public virtual void OnReactivate() { }
    public virtual void OnTerminate() { }
    public virtual void OnDisconnect() { }
}