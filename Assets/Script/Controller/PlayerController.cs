using Mono.Cecil;
using System;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    public TileMapManager TileMapManager { get; set; }

    private Define.CreatureState creatureState = Define.CreatureState.Idle;
    private Define.Direction direction = Define.Direction.Down;
    [SerializeField]
    public float speed = 3.0f;
    [SerializeField] private Animator animator;
    [SerializeField] public int bombCnt = 1;
    [SerializeField] public int bombPower = 1;
    private bool _didPlayTrapOnce;
    private bool _didPlayDeadOnce;
    private Vector2Int prevPos = Vector2Int.zero;

    private int _currentStateHash = 0;
    private const string trapStateName = "BazziTraap1Animation";
    private const string deadStateName = "Bazzi1DeadAnimation";
    // 상태 이름은 Animator 상태 이름과 동일해야 함
    private const string IdleState = "Bazzi1IdleAnimation";   // 너의 Idle 이름에 맞게 수정
    private const string MoveFrontState = "Bazzi1FrontAnimation";
    private const string MoveBackState = "Bazzi1BackAnimation";
    private const string MoveLeftState = "Bazzi1LeftAnimation";
    private const string MoveRightState = "Bazzi1RightAnimation";

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    void Start()
    {

    }

    void Update()
    {
        ReadInputWithKeys();
        UpdateFSM();        
    }

    private void ReadInputWithKeys() 
    {
        if (creatureState == Define.CreatureState.Traped || creatureState == Define.CreatureState.Dead) 
        {
            return;
        }

        var k = Keyboard.current;
        if (k == null) return;

        bool isMoving = false;

        if (k.spaceKey.isPressed)
        {
            TileMapInfo tileInfo = TileMapManager.GetCell(transform.position);
            if (tileInfo.TileMapInfomation != Define.TileMapInfomation.Empty)
            {
                // 빈곳에만 폭탄 설치 가능
                return;
            }
            else 
            {
                // 폭탄 설치 가능...
                BoomSet();
            }
        }


        if (k.leftArrowKey.isPressed)
        {
            creatureState = Define.CreatureState.Move;
            direction = Define.Direction.Left;
            isMoving = true;
        }
        else if (k.rightArrowKey.isPressed)
        {
            creatureState = Define.CreatureState.Move;
            direction = Define.Direction.Right;
            isMoving = true;
        }
        else if (k.upArrowKey.isPressed)
        {
            creatureState = Define.CreatureState.Move;
            direction = Define.Direction.Up;
            isMoving = true;
        }
        else if (k.downArrowKey.isPressed)
        {
            creatureState = Define.CreatureState.Move;
            direction = Define.Direction.Down;
            isMoving = true;
        }

        if (!isMoving)
        {
            creatureState = Define.CreatureState.Idle;
        }
    }

    private void BoomSet()
    {
        if (bombCnt <= 0) { return; }
        bombCnt -= 1;
        Vector2 tilePos = TileMapManager.ConvertWorldPosToTilePos(transform.position);
        GameObject boomPrefabs = Resources.Load<GameObject>("Prefabs/Boom");
        GameObject instance = Instantiate(boomPrefabs, new Vector2(tilePos.x + 0.5f, tilePos.y + 0.5f), Quaternion.identity);
        BoomController bc = instance.GetComponent<BoomController>();
        bc.blastRange = bombPower;
        bc.Owner = gameObject; // 폭탄의 소유권
    }

    private void UpdateFSM() 
    {
        switch (creatureState) 
        {
            case Define.CreatureState.Idle:
                UpdateIdle();
                break;

            case Define.CreatureState.Move:
                UpdateMove();
                break;
        }

        UpdateAnimation();
    }

    private void UpdateIdle()
    {
        
    }

    private void UpdateMove() 
    {
        // 실제로 움직임 
        Vector2 nowPos = transform.position;
        Vector2 dirVector2 = Vector2.up;
        Vector2Int prevLogicPos = TileMapManager.ConvertWorldPosToLogicPos(nowPos);

        if (direction == Define.Direction.Right) { dirVector2 = Vector2.right; }
        else if (direction == Define.Direction.Up) { dirVector2 = Vector2.up; }
        else if (direction == Define.Direction.Down) { dirVector2 = Vector2.down; }
        else if (direction == Define.Direction.Left) { dirVector2 = Vector2.left; }
        Vector2 nextPos = nowPos + (dirVector2 * Time.deltaTime * speed);           
        Vector2Int nextIndex = TileMapManager.ConvertWorldPosToLogicPos(nextPos);

        if (!TileMapManager.InBounds(nextIndex.x, nextIndex.y))
            return;

        TileMapInfo info = TileMapManager.GetCell(nextIndex.x, nextIndex.y);
        if (info.TileMapInfomation == Define.TileMapInfomation.WALL || info.TileMapInfomation == Define.TileMapInfomation.HardWall) 
        {
            return;
        }

        if (BombRegistry.TryGet(nextIndex, out var bomb)) 
        {
            if (bomb.Owner != gameObject || !bomb.Penetration) 
            {
                return;
            }
        }
        transform.position = nextPos;
        Vector2Int nowLogicPos = TileMapManager.ConvertWorldPosToLogicPos(nextPos);

        if (prevLogicPos != nowLogicPos) 
        {
            if (BombRegistry.TryGet(prevLogicPos, out var prevBomb)) 
            {
                if (prevBomb.Owner == gameObject && prevBomb.Penetration) 
                {
                    prevBomb.Penetration = false;
                }
            }
        }

        if (ItemRegistry.TryGet(nextIndex, out var item))
        {
            ItemController ic = item.GetComponent<ItemController>();
            ic.Use(this, nextIndex, TileMapManager); ;
        }
    }

    private void UpdateAnimation()
    {
        if (creatureState == Define.CreatureState.Idle)
        {
            // 마지막 프레임 멈춤 이미지 
            animator.speed = 0f;
            _didPlayTrapOnce = false; // 다음에 또 걸릴 수 있으니 초기화
            _didPlayDeadOnce = false;
        }
        else if (creatureState == Define.CreatureState.Traped)
        {
            if (!_didPlayTrapOnce)
            {
                _didPlayTrapOnce = true;
                StartCoroutine(CoPlayOnceAndHold(trapStateName, 0.2f)); // 천천히 재생
            }
        }
        else if (creatureState == Define.CreatureState.Dead) 
        {
            if (!_didPlayDeadOnce) 
            {
                _didPlayDeadOnce = true;
                StartCoroutine(CoPlayDeadOnceAndHold(deadStateName, 0.2f)); // 천천히 재생
            }
        }
        else
        {
            animator.speed = 1f;
            // 1) 현재 FSM/방향 → 재생할 상태 이름
            string nextStateName = GetAnimStateName(creatureState, direction);

            // 2) 같은 상태면 아무 것도 하지 않음 (리셋 방지)
            int nextHash = Animator.StringToHash(nextStateName);
            if (_currentStateHash == nextHash) return;

            // 3) 부드럽게 전환
            animator.CrossFade(nextHash, 0.08f, 0, 0f); // 0.08초 페이드, Layer 0
            _currentStateHash = nextHash;
        }
    }

    private static string GetAnimStateName(Define.CreatureState st, Define.Direction dir)
    {
        if (st == Define.CreatureState.Idle)
            return IdleState; // Idle을 방향별로 보여주고 싶다면 아래처럼 분기하면 됨

        // Move
        return dir switch
        {
            Define.Direction.Down => MoveFrontState,
            Define.Direction.Up => MoveBackState,
            Define.Direction.Left => MoveLeftState,
            Define.Direction.Right => MoveRightState,
            _ => MoveFrontState
        };
    }

    public void Traped() 
    {
        creatureState = Define.CreatureState.Traped;  
    }


    private IEnumerator CoPlayOnceAndHold(string stateName, float playSpeed = 1f)
    {
        // 1) 처음부터 재생
        animator.speed = playSpeed;
        animator.Play(stateName, 0, 0f);
        animator.Update(0f); // 즉시 평가(길이 계산 안정화용)

        // 2) 클립 길이 얻기
        float clipLen = FindClipLength(stateName);
        if (clipLen <= 0f)
        {
            // 못 찾으면 안전빵으로 0.5초 가정
            clipLen = 0.5f;
        }

        // 3) 끝까지 기다렸다가
        yield return new WaitForSeconds(clipLen / Mathf.Max(0.0001f, playSpeed));

        // 4) 마지막 프레임으로 점프 + 정지
        animator.Play(stateName, 0, 0.999f);
        animator.Update(0f);
        animator.speed = 0f;

        // 여기서 플레이어 상태갑 사망으로 변경
        creatureState = Define.CreatureState.Dead;
    }


    private IEnumerator CoPlayDeadOnceAndHold(string stateName, float playSpeed = 1f)
    {
        // 1) 처음부터 재생
        animator.speed = playSpeed;
        animator.Play(stateName, 0, 0f);
        animator.Update(0f); // 즉시 평가(길이 계산 안정화용)

        // 2) 클립 길이 얻기
        float clipLen = FindClipLength(stateName);
        if (clipLen <= 0f)
        {
            // 못 찾으면 안전빵으로 0.5초 가정
            clipLen = 0.5f;
        }

        // 3) 끝까지 기다렸다가
        yield return new WaitForSeconds(clipLen / Mathf.Max(0.0001f, playSpeed));

        // 4) 마지막 프레임으로 점프 + 정지
        animator.Play(stateName, 0, 0.999f);
        animator.Update(0f);
        animator.speed = 0f;

        TileMapManager.PlayerDict.Remove(1);
        Destroy(gameObject);
    }


    // AnimatorController에서 클립 길이 찾기
    private float FindClipLength(string clipName)
    {
        var rc = animator.runtimeAnimatorController;
        if (rc == null) return 0f;
        foreach (var clip in rc.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip.length;
        }
        return 0f;
    }
}
