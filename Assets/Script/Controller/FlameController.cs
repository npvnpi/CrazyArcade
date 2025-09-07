using System.Collections;
using System.Linq;
using UnityEngine;

public class FlameController : MonoBehaviour
{

    public TileMapManager TileMapManager { get; set; }
    private Vector2Int _logicPos = new Vector2Int(0, 0);
    private float _flameDuration = 0.0f;
    private bool _cleared;
    private Define.ItemInfomation _spawnItem = Define.ItemInfomation.None;   
    public void Init(TileMapManager tileMapManager, Vector2Int logicPos, float flameDuration) 
    {
        TileMapManager = tileMapManager;
        _logicPos = logicPos;
        _flameDuration = flameDuration;
        TileMapInfo prevTileMapInfo = TileMapManager.GetCell(logicPos.x, logicPos.y);

        TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Explosion);
        newTileMapInfo.itemInfomation = prevTileMapInfo.itemInfomation;
        TileMapManager.SetCell(logicPos, newTileMapInfo);
    }

    void Start()
    {
        StartCoroutine(CoFlame());
    }

    void Update()
    {
        // ÇÃ·¹ÀÌ¾î »ï¾Æ Ã¼Å©
        var playerGos = TileMapManager.PlayerDict.Values.ToArray();
        foreach (GameObject playerGo in playerGos) 
        {
            Vector3 playerPos = playerGo.transform.position;
            var playerLogciPos = TileMapManager.ConvertWorldPosToLogicPos(playerPos);
            if (_logicPos == playerLogciPos)
            {
                // ÇÃ·¹ÀÌ¾î ¹°Ç³¼± °¤Èû (°ÅÀÇ »ç¸Á Á÷Àü)
                var pc = playerGo.GetComponent<PlayerController>();
                pc.Traped();
            }
        }
    }

    private IEnumerator CoFlame()
    {
        string itemPrefabName = "";
        if (WallRegistry.TryGet(_logicPos, out GameObject wall))
        {
            if (TileMapManager.tileMapInfos[_logicPos.y, _logicPos.x].itemInfomation == Define.ItemInfomation.BombUp) 
            {
                _spawnItem = Define.ItemInfomation.BombUp;
                itemPrefabName = "Prefabs/bombUp";
            }
            else if (TileMapManager.tileMapInfos[_logicPos.y, _logicPos.x].itemInfomation == Define.ItemInfomation.PowerUpSmall)
            {
                _spawnItem = Define.ItemInfomation.PowerUpSmall;
                itemPrefabName = "Prefabs/powerSmall";
            }
            else if (TileMapManager.tileMapInfos[_logicPos.y, _logicPos.x].itemInfomation == Define.ItemInfomation.PowerUpBig)
            {
                _spawnItem = Define.ItemInfomation.PowerUpBig;
                itemPrefabName = "Prefabs/powerBig";
            }
            else if (TileMapManager.tileMapInfos[_logicPos.y, _logicPos.x].itemInfomation == Define.ItemInfomation.SpeedUp)
            {
                _spawnItem = Define.ItemInfomation.SpeedUp;
                itemPrefabName = "Prefabs/speedUp";
            }
            Destroy(wall);
            WallRegistry.Unregister(_logicPos);
        }

        if (ItemRegistry.TryGet(_logicPos, out GameObject item)) 
        {
            Destroy(item);
            ItemRegistry.Unregister(_logicPos);
            TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Item);
            newTileMapInfo.itemInfomation = Define.ItemInfomation.None;
            TileMapManager.SetCell(_logicPos, newTileMapInfo);
        }


        yield return new WaitForSeconds(_flameDuration);
        ClearTileOnce();
        Destroy(gameObject);

        if (_spawnItem != Define.ItemInfomation.None)
        {
            TileMapInfo newTileMapInfo = new TileMapInfo(Define.TileMapInfomation.Item);
            newTileMapInfo.itemInfomation = _spawnItem;
            TileMapManager.SetCell(_logicPos, newTileMapInfo);
            Vector2 pos = TileMapManager.ConvertLogicPosToWorldPos(_logicPos);
            GameObject itemPrefab = Resources.Load<GameObject>(itemPrefabName);
            GameObject itemGameObject = Instantiate(itemPrefab, pos, Quaternion.identity, TileMapManager.gameObject.transform);
            ItemController ic = itemGameObject.AddComponent<ItemController>();
            ic.ItemType = _spawnItem;
            ItemRegistry.Register(itemGameObject, _logicPos);
        }
    }

    private void ClearTileOnce()
    {
        if (_cleared) return;
        _cleared = true;
        if (TileMapManager != null) {
            TileMapManager.SetCell(_logicPos, new TileMapInfo(Define.TileMapInfomation.Empty));
        }
    }

    private void OnDestroy()
    {
        ClearTileOnce();
    }
}
