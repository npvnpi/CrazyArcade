using System.Collections;
using UnityEngine;

public class FlameController : MonoBehaviour
{

    public TileMapManager TileMapManager { get; set; }
    private Vector2Int _logicPos = new Vector2Int(0, 0);
    private float _flameDuration = 0.0f;
    private bool _cleared;
    public void Init(TileMapManager tileMapManager, Vector2Int logicPos, float flameDuration) 
    {
        TileMapManager = tileMapManager;
        _logicPos = logicPos;
        _flameDuration = flameDuration;
        TileMapManager.SetCell(logicPos, new TileMapInfo(Define.TileMapInfomation.Explosion));
    }

    void Start()
    {
        StartCoroutine(CoFlame());
    }

    void Update()
    {
        
    }

    private IEnumerator CoFlame()
    {
        yield return new WaitForSeconds(_flameDuration);
        ClearTileOnce();
        Destroy(gameObject);
    }

    private void ClearTileOnce()
    {
        if (_cleared) return;
        _cleared = true;
        if (TileMapManager != null)
            TileMapManager.SetCell(_logicPos, new TileMapInfo(Define.TileMapInfomation.Empty));
    }

    private void OnDestroy()
    {
        ClearTileOnce();
    }
}
