using System.Collections;
using UnityEngine;

public class BoomController : MonoBehaviour
{
    [SerializeField] private float fuseSeconds = 2.5f;        // 도화선 시간
    [SerializeField] private int blastRange = 1;               // 물줄기 길이(타일 수)
    [SerializeField] private float flameDuration = 0.35f;      // 물줄기 유지 시간
    public TileMapManager TileMapManager { get; set; }
    private bool _exploded;
    private Vector2Int _logicPos = new Vector2Int(0,0);

    void Start()
    {
        if (!TileMapManager) TileMapManager = FindAnyObjectByType<TileMapManager>();
        Vector2Int tilePos = TileMapManager.ConvertWorldPosToLogicPos(transform.position);
        TileMapManager.tileMapInfos[tilePos.y, tilePos.x] = new TileMapInfo(Define.TileMapInfomation.Boom);
        _logicPos = tilePos;
        StartCoroutine(CoFuse());
    }

    void Update()
    {
        
    }

    private IEnumerator CoFuse()
    {
        float t = 0f;
        while (t < fuseSeconds && !_exploded)
        {
            t += Time.deltaTime;
            yield return null;
        }
        Explode();
    }

    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        // 타일 마킹 되돌리기(폭탄 제거)
        TileMapManager.SetCell(_logicPos.x, _logicPos.y, new TileMapInfo(Define.TileMapInfomation.Empty));

        // 사운드
        // if (sfxExplode) sfxExplode.Play();

        // 중앙 화염
        // SpawnFlame(flameCenterPrefab, _logicPos, FlameKind.Center);

        // 네 방향으로 레이 쏘듯 확장
        // Spread(Vector2Int.up);
        // Spread(Vector2Int.down);
        // Spread(Vector2Int.left);
        // Spread(Vector2Int.right);

        // 폭탄 본체 삭제 (효과는 코루틴으로 알아서 사라짐)
        Destroy(gameObject);
    }

    private void SpawnFlame(GameObject prefab, Vector2Int logicPos) 
    {
        if (!prefab) return;

        Vector2 world = TileMapManager.ConvertTilePosToWorldPos(logicPos);
        Vector3 pos = new Vector3(world.x + 0.5f, world.y + 0.5f, 0f);
    }


}
