using System.Collections;
using UnityEngine;

public class BoomController : MonoBehaviour
{
    [SerializeField] private float fuseSeconds = 2.5f;        // ��ȭ�� �ð�
    [SerializeField] private int blastRange = 1;               // ���ٱ� ����(Ÿ�� ��)
    [SerializeField] private float flameDuration = 0.35f;      // ���ٱ� ���� �ð�
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

        // Ÿ�� ��ŷ �ǵ�����(��ź ����)
        TileMapManager.SetCell(_logicPos.x, _logicPos.y, new TileMapInfo(Define.TileMapInfomation.Empty));

        // ����
        // if (sfxExplode) sfxExplode.Play();


        GameObject spreadCenterPrefab = Resources.Load<GameObject>("Prefabs/SpreadCenter");
        // �߾� ȭ��
        SpawnFlame(spreadCenterPrefab, _logicPos);

        // �� �������� ���� ��� Ȯ��
        Spread(Vector2Int.up);
        Spread(Vector2Int.down);
        Spread(Vector2Int.left);
        Spread(Vector2Int.right);

        // ��ź ��ü ���� (ȿ���� �ڷ�ƾ���� �˾Ƽ� �����)
        Destroy(gameObject);
    }

    private void Spread(Vector2Int dir) 
    {
        for (int i = 1; i <= blastRange; i++) 
        {
            var p = _logicPos + (dir * i);
            Debug.Log(p);
            if (TileMapManager.IsHardBlock(p)) 
                break;

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
