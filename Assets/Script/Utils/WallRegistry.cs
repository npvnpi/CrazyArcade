using System.Collections.Generic;
using UnityEngine;

public class WallRegistry
{
    private static readonly Dictionary<Vector2Int, GameObject> _map = new();

    public static void Register(GameObject wall, Vector2Int pos)
    {
        _map[pos] = wall;
    }

    public static void Unregister(Vector2Int pos)
    {
        _map.Remove(pos);
    }

    public static bool TryGet(Vector2Int pos, out GameObject wall)
        => _map.TryGetValue(pos, out wall);
}
