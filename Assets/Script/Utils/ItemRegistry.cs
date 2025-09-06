using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemRegistry
{
    private static readonly Dictionary<Vector2Int, GameObject> _map = new();

    public static void Register(GameObject item, Vector2Int pos)
    {
        _map[pos] = item;
    }

    public static void Unregister(Vector2Int pos)
    {
        _map.Remove(pos);
    }

    public static bool TryGet(Vector2Int pos, out GameObject item)
        => _map.TryGetValue(pos, out item);
}
