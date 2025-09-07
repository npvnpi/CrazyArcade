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
        Traped,
        Dead
    }

    public enum TileMapInfomation
    {
        Empty,
        WALL,
        HardWall,
        Boom,
        Explosion,
        Item
    }

    public enum ItemInfomation 
    {
        None,
        BombUp,
        PowerUpBig,
        PowerUpSmall,
        SpeedUp
    }
}
