using System.Collections.Generic;
using UnityEngine;

public class BombRegistry
{
    private static readonly Dictionary<Vector2Int, BoomController> _map = new();

    public static void Register(BoomController bomb, Vector2Int pos)
    {
        _map[pos] = bomb;
    }

    public static void Unregister(Vector2Int pos, BoomController bomb)
    {
        if (_map.TryGetValue(pos, out var cur) && cur == bomb)
            _map.Remove(pos);
    }

    public static bool TryGet(Vector2Int pos, out BoomController bomb)
        => _map.TryGetValue(pos, out bomb);
}
