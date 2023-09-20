using System.Runtime.InteropServices;

namespace Touhou;

[Flags]
public enum PlayerActions : int {
    None = 0,
    Right = 1,
    Left = 2,
    Down = 4,
    Up = 8,
    Focus = 16,
    Primary = 32,
    Secondary = 64,
    SpellA = 128,
    SpellB = 256,
    Bomb = 512
}