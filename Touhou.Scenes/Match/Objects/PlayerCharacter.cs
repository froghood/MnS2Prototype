// using SFML.Graphics;
// using SFML.System;
// using Touhou.Net;
// using Touhou.Objects;

// namespace Touhou.Scenes.Match.Objects;


// public class PlayerCharacter {

//     public float Speed { get; private set; }
//     public float FocusedSpeed { get; private set; }
//     public Color Color { get; private set; }

//     private Player _player;

//     private float _attackHoldDuration;
//     private HoldState _attackHoldState;

//     private float _normalizedAimOffset;
//     private float _aimOffset;

//     private Action<long, float> _onAttackUpdate;

//     public PlayerCharacter(Player player) {
//         _player = player;
//         Speed = 500f;
//         FocusedSpeed = 200f;
//     }

//     public void Press(ActionData action) {
//         if (_attackHoldState != HoldState.None) return;
//         switch (action) {
//             case PlayerAction.AttackA:
//                 _attackHoldState = HoldState.Primary;
//                 _onAttackUpdate = OnPrimaryUpdate;
//                 break;
//             case PlayerAction.AttackB:
//                 _attackHoldState = HoldState.Secondary;
//                 break;
//             case PlayerAction.SpellA:
//                 _attackHoldState = HoldState.SpellA;
//                 break;
//             case PlayerAction.SpellB:
//                 _attackHoldState = HoldState.SpellB;
//                 break;
//         }
//     }

//     public void Release(ActionData action) {
//         switch (_attackHoldState) {
//             case HoldState.Primary:
//                 if (action != PlayerAction.AttackA) break;
//                 Primary();
//                 _attackHoldState = HoldState.None;
//                 _onAttackUpdate = null;
//                 _attackHoldDuration = 0f;
//                 _normalizedAimOffset = 0f;
//                 break;
//             case HoldState.Secondary:
//                 if (action != PlayerAction.AttackB) break;
//                 _attackHoldState = HoldState.None;
//                 _onAttackUpdate = null;
//                 _attackHoldDuration = 0f;
//                 _normalizedAimOffset = 0f;
//                 break;
//             case HoldState.SpellA:
//                 if (action != PlayerAction.SpellA) break;
//                 _attackHoldState = HoldState.None;
//                 _onAttackUpdate = null;
//                 _attackHoldDuration = 0f;
//                 _normalizedAimOffset = 0f;
//                 break;
//             case HoldState.SpellB:
//                 if (action != PlayerAction.SpellB) break;
//                 _attackHoldState = HoldState.None;
//                 _onAttackUpdate = null;
//                 _attackHoldDuration = 0f;
//                 _normalizedAimOffset = 0f;
//                 break;
//         }
//     }

//     private void Primary() {
//         var numShots = 5;
//         var speed = _player.Focused ? 250f : 150f;
//         //var arc = MathF.Tau / numShots;
//         var arc = 0.3f;
//         var angle = _player.AngleToOpponent + _aimOffset;

//         for (int i = 0; i < numShots; i++) {

//             var projectile = new LinearAmulet(_player.Position, angle + arc * i - 0.2f, 6f, 2f, 0.25f);
//             _player.Scene.AddEntity(projectile);

//             //_player.Scene.Projectiles.CreateLinearAmulet(false, false, _player.Position, angle + arc * i - 0.2f, 6f, 2f, 0.25f);

//             // var projectileB = new Bullet(_player.Position, angle + arc * i, speed, -1);
//             // _player.GameManager.CreatePlayerProjectile(projectileB);
//         }

//         var packet = new Packet(PacketType.Primary).In(Game.Network.Time).In(_player.Position).In(angle).In(_player.Focused);
//         Game.Network.Send(packet);
//     }

//     public void Update() {
//         _onAttackUpdate?.Invoke();
//     }

//     private void OnPrimaryUpdate() {

//         float aimRange = MathF.PI / 180f * 140f;
//         float aimStrength = 0.1f;
//         float gamma = 1 - MathF.Pow(aimStrength, delta);

//         if (_attackHoldDuration > 0.12f) { // ~7 frames at 60fps

//             var arcLengthToVelocity = TMathF.NormalizeAngle(_player.VelocityAngle - TMathF.NormalizeAngle(_player.AngleToOpponent + _normalizedAimOffset * aimRange));

//             if (_player.Moving) {
//                 _normalizedAimOffset -= _normalizedAimOffset * gamma;
//                 _normalizedAimOffset += MathF.Abs(arcLengthToVelocity / aimRange) < gamma ? arcLengthToVelocity / aimRange : gamma * MathF.Sign(arcLengthToVelocity);

//                 //_normalizedAimOffset += MathF.Min(gamma * MathF.Sign(arcLengthToVelocity), arcLengthToVelocity / aimRange);

//             } else {
//                 _normalizedAimOffset -= _normalizedAimOffset * 0.1f;
//             }
//         }

//         _aimOffset = _normalizedAimOffset * aimRange;
//         _attackHoldDuration += delta;
//     }

//     public void Render() {

//         int numVertices = 32;
//         float aimRange = MathF.PI / 180f * 140f;
//         float fullRange = aimRange * 2;
//         float increment = fullRange / (numVertices - 1);

//         if (_attackHoldDuration > 0.12f) { // ~7 frames at 60fps
//             var vertexArray = new VertexArray(PrimitiveType.TriangleFan);
//             vertexArray.Append(new Vertex(_player.Position, new Color(255, 255, 255, 50)));
//             for (int i = 0; i < numVertices; i++) {
//                 vertexArray.Append(new Vertex(_player.Position + new Vector2f(
//                     MathF.Cos(_player.AngleToOpponent + aimRange - increment * i) * 40f,
//                     MathF.Sin(_player.AngleToOpponent + aimRange - increment * i) * 40f
//                 ), new Color(255, 255, 255, 10)));
//             }
//             Game.Window.Draw(vertexArray);

//             var shape = new RectangleShape(new Vector2f(40f, 2f));
//             shape.Origin = new Vector2f(0f, 1f);
//             shape.Position = _player.Position;
//             shape.Rotation = 180f / MathF.PI * (_player.AngleToOpponent + _aimOffset);
//             shape.FillColor = new Color(255, (byte)MathF.Round(255f - 100f * MathF.Abs(_normalizedAimOffset)), (byte)MathF.Round(255f - 100f * Math.Abs(_normalizedAimOffset)));
//             Game.Window.Draw(shape);
//         }
//     }

// }