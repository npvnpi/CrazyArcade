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

            //public TileBase boxTile;         // 1
            //public TileBase orangeTile;      // 2
            //public TileBase redTile;         // 3
            //public TileBase redHouseTile;    // 4
            //public TileBase yellowHouseTile; // 5
            //public TileBase blueHouseTile;   // 6
            //public TileBase treeTile;        // 7

    int[,] Map =
       {
            // y=0
            {0,0,1,1,1,1,1,2,3,3,3,1,1,1,1,1,0,0},
            // y=1
            {0,0,1,1,2,1,1,4,1,5,1,6,1,1,2,1,0,0},
            // y=2
            {1,1,1,1,1,1,1,2,3,1,1,2,1,2,1,1,1,1},
            // y=3
            {1,2,2,1,1,1,1,7,1,1,7,1,1,1,1,2,2,1},
            // y=4
            {1,2,1,1,1,1,0,0,0,0,0,0,1,1,1,1,2,1},
            // y=5
            {1,1,1,2,1,1,0,0,0,0,0,0,1,1,2,1,1,1},
            // y=6
            {1,3,1,1,1,1,7,1,1,1,1,7,1,1,1,1,3,1},
            // y=7
            {1,2,2,1,1,1,1,2,3,1,1,1,1,1,2,2,1,1},
            // y=8
            {0,0,1,1,6,1,1,5,1,4,1,1,1,1,6,1,0,0},
            // y=9
            {0,0,1,1,1,1,1,2,3,3,3,1,1,1,1,1,0,0}
        };

        for (int y = 0; y < HEIGHT; y++)
        {
            for (int x = 0; x < WIDTH; x++)
            {
                int data = Map[y, x];
                if (data == 1)
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/BoxBlock");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x,y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x,y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.WALL);
                }
                else if (data == 2)
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/OrangeBlock");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x, y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x, y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.WALL);
                }
                else if (data == 3)
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/RedBlock");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x, y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x, y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.WALL);
                }
                else if (data == 4)
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/RedHouseBlock");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x, y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x, y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.HardWall);
                }
                else if (data == 5) 
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/YellowHouseBlock");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x, y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x, y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.HardWall);
                }
                else if (data == 6)
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/BlueHouseBlock");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x, y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x, y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.HardWall);
                }
                else if (data == 7)
                {
                    GameObject boxBlockPrefab = Resources.Load<GameObject>("Prefabs/Tree2Block");
                    Vector2 pos = ConvertLogicPosToWorldPos(new Vector2Int(x, y));
                    WallRegistry.Register(Instantiate(boxBlockPrefab, pos, Quaternion.identity, transform), new Vector2Int(x, y));
                    tileMapInfos[y, x] = new TileMapInfo(Define.TileMapInfomation.HardWall);
                }
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
