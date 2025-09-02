using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.Tilemaps;

public class TileMapManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap tilemap;
    [SerializeField]
    private Vector2Int originCell;

    public const int X_MIN = 0, X_MAX = 17; 
    public const int Y_MIN = 0, Y_MAX = 9; 
    public const int WIDTH = X_MAX - X_MIN + 1;  
    public const int HEIGHT = Y_MAX - Y_MIN + 1;  

    public TileMapInfo[,] tileMapInfos = new TileMapInfo[HEIGHT, WIDTH]; // [y, x]

    private void Awake()
    {
        for (int y = 0; y < HEIGHT; y++) 
        {
            for (int x = 0; x < WIDTH; x++) 
            {
                tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.Empty);
            }
        }
    }

    void Start()
    {
        for (int y = 0; y < HEIGHT; y++) 
        {
            for (int x = 0; x < WIDTH; x++) 
            {
                GameObject TownGround = Resources.Load<GameObject>("Prefabs/TownGround");
                Vector2 pos = ConvertLogicPosToTilePos(new Vector2Int(x, y));
                Instantiate(TownGround, pos + new Vector2(0.5f, 0.5f), Quaternion.identity, transform);
            }
        }

        GameObject prefab = Resources.Load<GameObject>("Prefabs/Bazzi");

        if (prefab != null)
        {
            Vector2 pos = originCell + new Vector2(0.5f, 0.5f);

            GameObject instance = Instantiate(prefab, pos, Quaternion.identity, transform);
            PlayerController pc = instance.GetComponent<PlayerController>();
            pc.TileMapManager = this;
            instance.name = "Bazzi";
        }
    }

    void Update()
    {
        
    }

    public bool InBounds(int x, int y) 
    {
        return x >= X_MIN && x <= X_MAX && y >= Y_MIN && y < Y_MAX;
    }

    public TileMapInfo GetCell(int x, int y)
    {
        return tileMapInfos[y, x];
    }

    public TileMapInfo GetCell(Vector3 pos)
    {
        Vector2Int boardPos = ConvertWorldPosToLogicPos(pos);
        return tileMapInfos[boardPos.y, boardPos.x];
    }

    public void SetCell(int x, int y, TileMapInfo info)
    {
        if (!InBounds(x, y)) return;
        tileMapInfos[y, x] = info;
    }

    public void SetCell(Vector2Int pos, TileMapInfo info)
    {
        if (!InBounds(pos.x, pos.y)) return;
        tileMapInfos[pos.y, pos.x] = info;
    }

    public Vector2Int ConvertWorldPosToLogicPos(Vector2 pos) 
    {
        Vector3Int ret = tilemap.WorldToCell(pos); 
        int convertX = ret.x - originCell.x;
        int convertY = ret.y - originCell.y;
        return new Vector2Int(convertX, convertY);
    }

    public Vector2Int ConvertWorldPosToTilePos(Vector2 pos)
    {
        Vector3Int ret = tilemap.WorldToCell(pos);
        return new Vector2Int(ret.x, ret.y);
    }

    public Vector2 ConvertTilePosToWorldPos(Vector2 pos) 
    {
        float convertX = pos.x + originCell.x;
        float convertY = pos.y + originCell.y;
        return new Vector2(convertX, convertY);
    }

    public Vector2Int ConvertLogicPosToTilePos(Vector2Int pos) 
    {
        Vector2Int tileVector = new Vector2Int(pos.x + originCell.x, pos.y + originCell.y);
        return tileVector;
    }

    public Vector2 ConvertLogicPosToWorldPos(Vector2Int pos)
    {
        Vector2 tilePos = ConvertTilePosToWorldPos(pos);
        return new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f);
    }

    public bool IsHardBlock(Vector2Int pos) 
    {
        if (!InBounds(pos.x, pos.y))
            return true;

        TileMapInfo info = tileMapInfos[pos.y, pos.x];

        if (info == null)
            return false;

        if (info.TileMapInfomation == Define.TileMapInfomation.HardWall)
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
}
