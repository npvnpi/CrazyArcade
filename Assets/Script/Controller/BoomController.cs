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

        // �߾� ȭ��
        // SpawnFlame(flameCenterPrefab, _logicPos, FlameKind.Center);

        // �� �������� ���� ��� Ȯ��
        // Spread(Vector2Int.up);
        // Spread(Vector2Int.down);
        // Spread(Vector2Int.left);
        // Spread(Vector2Int.right);

        // ��ź ��ü ���� (ȿ���� �ڷ�ƾ���� �˾Ƽ� �����)
        Destroy(gameObject);
    }

    private void SpawnFlame(GameObject prefab, Vector2Int logicPos) 
    {
        if (!prefab) return;

        Vector2 world = TileMapManager.ConvertTilePosToWorldPos(logicPos);
        Vector3 pos = new Vector3(world.x + 0.5f, world.y + 0.5f, 0f);
    }


}
