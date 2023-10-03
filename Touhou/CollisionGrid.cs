using System.Collections;
using OpenTK.Mathematics;
using Touhou;
using Touhou.Objects;

namespace Touhou;

public class CollisionGrid {

    public int Width { get; private set; }
    public int Height { get; private set; }

    public float CellWidth { get; private set; }
    public float CellHeight { get; private set; }


    private List<Hitbox>[] _grid;

    private Dictionary<(int X, int Y), List<Hitbox>> _gridDict = new();

    public CollisionGrid(int width, int height) {
        _grid = new List<Hitbox>[width * height];
        Width = width;
        Height = height;
        CellWidth = Game.WindowSize.X / (float)Width;
        CellHeight = Game.WindowSize.Y / (float)Height;
    }

    public void Add(Hitbox hitbox, int x, int y) {
        if (_gridDict.TryGetValue((x, y), out var list)) {
            list.Add(hitbox);
        } else {
            list = new List<Hitbox>() { hitbox };
            _gridDict.Add((x, y), list);
        }
        // var list = _grid[y * Width + x];
        // if (list == null) list = new List<T>();
        // list.Add(element);
    }

    public bool TryGet(int x, int y, out List<Hitbox> list) {
        //Log.Info($"{x}, {y}");
        if (_gridDict.TryGetValue((x, y), out var _list)) {
            list = _list;
            return true;
        }
        list = null;
        return false;

        // var list = _grid[y * Width + x];
        // if (list == null) list = new List<T>();
        // return list;
    }

    public void Clear() {
        _gridDict.Clear();

        // foreach (var list in _grid) {
        //     list?.Clear();
        // }
    }

    public void Render() {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Height; y++) {
                var position = new Vector2(x * CellWidth, y * CellHeight);
                var size = new Vector2(CellWidth, CellHeight);
                //Game.Debug.DrawRect(position, size, new Color4(255, 255, 255, (byte)(5 * ((x + y) % 2))), new Vector2(0f, 0f));

                if (TryGet(x, y, out var list)) {
                    //Game.Debug.DrawRectOutline(position, size, Color4.White);

                    foreach (var hitbox in list) {
                        //Game.Debug.DrawLine(position + size / 2f, hitbox.Position, new Color4(0, 200, 255));
                    }
                }
            }
        }
    }
}