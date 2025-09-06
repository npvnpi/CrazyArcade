using UnityEngine;
using static UnityEditor.Progress;

public class ItemController : MonoBehaviour
{
    public Define.ItemInfomation ItemType { get; set; } = Define.ItemInfomation.None;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Use(PlayerController pc, Vector2Int nextIndex, TileMapManager TileMapManager) 
    {
        if (ItemType == Define.ItemInfomation.BombUp)
        {
            pc.bombCnt += 1;
            Destroy(gameObject);
            ItemRegistry.Unregister(nextIndex);
            TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Empty);
            newTileMapInfo.itemInfomation = Define.ItemInfomation.None;
            TileMapManager.tileMapInfos[nextIndex.y, nextIndex.x] = newTileMapInfo;

        }
        else if (ItemType == Define.ItemInfomation.PowerUpSmall)
        {
            pc.bombCnt += 1;
            Destroy(gameObject);
            ItemRegistry.Unregister(nextIndex);
            TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Empty);
            newTileMapInfo.itemInfomation = Define.ItemInfomation.None;
            TileMapManager.tileMapInfos[nextIndex.y, nextIndex.x] = newTileMapInfo;
        }
        else if (ItemType == Define.ItemInfomation.PowerUpBig)
        {
            pc.bombCnt += 1;
            Destroy(gameObject);
            ItemRegistry.Unregister(nextIndex);
            TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Empty);
            newTileMapInfo.itemInfomation = Define.ItemInfomation.None;
            TileMapManager.tileMapInfos[nextIndex.y, nextIndex.x] = newTileMapInfo;
        }
        else if (ItemType == Define.ItemInfomation.SpeedUp)
        {
            pc.bombCnt += 1;
            Destroy(gameObject);
            ItemRegistry.Unregister(nextIndex);
            TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Empty);
            newTileMapInfo.itemInfomation = Define.ItemInfomation.None;
            TileMapManager.tileMapInfos[nextIndex.y, nextIndex.x] = newTileMapInfo;
        }
    }
}
