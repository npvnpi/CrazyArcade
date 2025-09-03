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

    public GameObject Owner { get; set; }
    public bool Penetration { get; set; } = true;

    void Start()
    {
        if (!TileMapManager) TileMapManager = FindAnyObjectByType<TileMapManager>();
        Vector2Int tilePos = TileMapManager.ConvertWorldPosToLogicPos(transform.position);
        TileMapManager.tileMapInfos[tilePos.y, tilePos.x] = new TileMapInfo(Define.TileMapInfomation.Boom);
        _logicPos = tilePos;
        BombRegistry.Register(this, _logicPos);
        StartCoroutine(CoFuse());
    }

    void Update()
    {
        
    }

    private IEnumerator CoFuse()
    {
        yield return new WaitForSeconds(fuseSeconds);
        Explode();
    }

    // 체인 반응용: 즉시 폭발
    public void TriggerChain()
    {
        if (_exploded) return;
        StopAllCoroutines(); // 남은 도화선 무효화
        Explode();
    }


    private void Explode()
    {
        if (_exploded) return;
        _exploded = true;

        BombRegistry.Unregister(_logicPos, this);
        // 타일 마킹 되돌리기(폭탄 제거)
        TileMapManager.SetCell(_logicPos.x, _logicPos.y, new TileMapInfo(Define.TileMapInfomation.Empty));

        // 사운드
        // if (sfxExplode) sfxExplode.Play();


        GameObject spreadCenterPrefab = Resources.Load<GameObject>("Prefabs/SpreadCenter");
        // 중앙 화염
        SpawnFlame(spreadCenterPrefab, _logicPos);

        // 네 방향으로 레이 쏘듯 확장
        Spread(Vector2Int.up);
        Spread(Vector2Int.down);
        Spread(Vector2Int.left);
        Spread(Vector2Int.right);

        // 폭탄 본체 삭제 (효과는 코루틴으로 알아서 사라짐)
        Destroy(gameObject);
    }

    private void Spread(Vector2Int dir) 
    {
        for (int i = 1; i <= blastRange; i++) 
        {
            var p = _logicPos + (dir * i);
   
            if (TileMapManager.IsHardBlock(p)) 
                break;

            // ① 그 칸에 폭탄이 있으면 즉시 체인 유발
            if (BombRegistry.TryGet(p, out var otherBomb))
            {
                otherBomb.TriggerChain();
                break;
            }


            string prefabPath = "";

            if (dir == Vector2Int.up)
            {
                prefabPath = "SpreadUp";
            }
            else if (dir == Vector2Int.down)
            {
                prefabPath = "SpreadDown";
            }
            else if (dir == Vector2Int.left) 
            {
                prefabPath = "SpreadLeft";
            }
            else if (dir == Vector2Int.right)
            {
                prefabPath = "SpreadRight";
            }

            var prefab = Resources.Load<GameObject>($"Prefabs/{prefabPath}");
            var flame = SpawnFlame(prefab, p);

            //if (TileMapManager.IsBreakable(p))
            //{
            //    TileMapManager.Break(p);
            //    break;
            //}
        }
    }

    private FlameController SpawnFlame(GameObject prefab, Vector2Int logicPos) 
    {
        if (!prefab) 
            return null;
        Vector2 worldPos = TileMapManager.ConvertLogicPosToWorldPos(logicPos);
        // Debug.Log(worldPos);
        GameObject spreadFlame = Instantiate(prefab, worldPos, Quaternion.identity);
        FlameController fc = spreadFlame.GetComponent<FlameController>();
        fc.Init(TileMapManager, logicPos, flameDuration);

        return fc;
    }


}
