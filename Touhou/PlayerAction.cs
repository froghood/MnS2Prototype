using System.Runtime.InteropServices;

namespace Touhou;
public enum PlayerAction : int {
    None = 0,
    Right,
    Left,
    Down,
    Up,
    Focus,
    Primary,
    Secondary,
    SpellA,
    SpellB,
    Bomb
}