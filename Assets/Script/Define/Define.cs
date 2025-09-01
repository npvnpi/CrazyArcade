using UnityEngine;

public class Define
{
    public enum Direction 
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum CreatureState 
    {
        Idle,
        Move,
    }

    public enum TileMapInfomation
    {
        Empty,
        WALL,
        Boom,
        Explosion
    }
}
