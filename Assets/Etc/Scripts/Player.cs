using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerAction
{
    public string action;
    public string target;
    public Position position;
    public bool success;
    public string message;
}

[Serializable]
public class Position
{
    public float x;
    public float y;
}

public static class PlayerActionParser
{
    public static PlayerAction Parse(string json)
    {
        try
        {
            PlayerAction action = JsonUtility.FromJson<PlayerAction>(json);

            return action;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JSON 파싱 오류] {e.Message}");
            return null;
        }
    }
}

public class ActionExecutor
{
    private readonly Player player;
    private readonly PlayerStatusSO status;

    public ActionExecutor(Player player, PlayerStatusSO status)
    {
        this.player = player;
        this.status = status;
    }

    public void Execute(PlayerAction action)
    {
        switch (action.action)
        {
            case "move":
                // 이동 후 실제 거리 계산
                Vector2 start = player.transform.position;
                Vector2 target = new Vector2(action.position.x, action.position.y);
                float distance = Vector2.Distance(start, target);
                player.MoveTo(new Vector2(action.position.x, action.position.y));

                // PlayerStatusSO가 자체적으로 스탯 감소 처리
                status.ApplyMovementCost(distance);
                break;

            case "rest":
                status.ApplyRestEffect();
                break;

            case "eat":
                // target 이름이 있을 경우 해당 FoodData를 검색
                if (!string.IsNullOrEmpty(action.target))
                {
                    FoodDataSO food = FoodDataManager.instance?.FindFoodByName(action.target);
                    if (food != null)
                    {
                        status.ApplyEatEffect(food);
                        Debug.Log($"[먹기] {food.foodName} 섭취 완료");
                    }
                    else
                    {
                        Debug.LogWarning($"[먹기 실패] '{action.target}' 음식 데이터를 찾지 못했습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("[먹기 실패] target 이름이 지정되지 않았습니다.");
                }
                break;

            default:
                Debug.Log($"[알 수 없는 행동] {action.action}");
                break;
        }

        status.NextTurn();
    }
}





public class Player : MonoBehaviour
{
    [Header("이동 속도 (칸 단위 이동)")]
    public float moveSpeed = 2f;

    [Header("플레이어 상태 데이터 (ScriptableObject)")]
    public PlayerStatusSO playerStatus;   // PlayerStatusSO 연결

    private ActionExecutor actionExecutor;
    private Vector2 currentPosition;
    public Vector2 CurrentPosition => currentPosition;
    private Coroutine moveCoroutine;
    private bool isAttacking = false;





    void Start()
    {
        currentPosition = transform.position;

        if (playerStatus == null)
        {
            Debug.LogWarning("PlayerStatusSO가 연결되지 않았습니다!");
            return;
        }

        // 복제본 생성
        playerStatus = Instantiate(playerStatus);
        playerStatus.ResetStatus();

        // StatManager에 복제본 전달
        if (StatManager.instance != null)
            StatManager.instance.SetPlayerStatus(playerStatus);

        // ActionExecutor 초기화
        actionExecutor = new ActionExecutor(this, playerStatus);
    }

    void OnEnable()
    {
        TestAI.OnAIResponse += ApplyActionJSON; // 이벤트 구독
    }

    void OnDisable()
    {
        TestAI.OnAIResponse -= ApplyActionJSON; // 메모리 해제
    }

    void Update()
    {
        if (!isAttacking)
            TryAttackNearbyEnemies();
    }







    /// <summary>
    /// JSON에서 좌표 데이터만 추출
    /// (target 이름 해석은 외부에서 처리)
    /// </summary>
    public (bool success, string actionType, string target, string message, Vector2 position) ParseTargetPosition(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
            {
                Debug.LogWarning("응답이 JSON 형식이 아닙니다. 원본 출력: " + json);
                return (false, "unknown", "none", "(JSON 형식 아님)", Vector2.zero);
            }

            JObject jObj = JObject.Parse(json);

            string actionType = jObj["action"]?.ToString() ?? "unknown";
            string target = jObj["target"]?.ToString() ?? "none";
            bool success = jObj["success"]?.Value<bool>() ?? false;
            string message = jObj["message"]?.ToString() ?? "(메시지 없음)";

            float x = jObj["position"]?["x"]?.Value<float>() ?? 0f;
            float y = jObj["position"]?["y"]?.Value<float>() ?? 0f;

            Vector2 position = new Vector2(x, y);

            // target 포함한 튜플 반환
            return (success, actionType, target, message, position);
        }
        catch (Exception ex)
        {
            Debug.LogError($"JSON 파싱 중 오류 발생: {ex.Message}");
            Debug.Log("문제 발생한 응답: " + json);
            return (false, "error", "none", ex.Message, Vector2.zero);
        }
    }




    /// <summary>
    /// 외부에서 좌표(Vector2)를 받아 이동 시작
    /// </summary>
    public void MoveTo(Vector2 target)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Vector2 target)
    {
        Vector2 startPos = transform.position;
        float distance = Vector2.Distance(startPos, target);
        float duration = distance / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector2.Lerp(startPos, target, t);
            yield return null;
        }

        // 이동 완료 후 위치 갱신
        transform.position = target;
        currentPosition = target; // 현재 좌표 업데이트

        moveCoroutine = null;

        // 이동이 끝난 뒤 근처 적 탐색 및 전투
        TryAttackNearbyEnemies();
    }

    /// <summary>
    /// 주변 Enemy 오브젝트 탐색 후 전투 수행
    /// </summary>
    private void TryAttackNearbyEnemies()
    {
        if (isAttacking) return;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        while (true)
        {
            // 공격 가능한 적 검색
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, playerStatus.AttackRange);
            bool attacked = false;

            foreach (Collider2D hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && !enemy.enemyStatus.IsDead)
                {
                    Debug.Log($"플레이어가 [{enemy.enemyData.objectName}]을(를) 공격합니다!");
                    int damage = playerStatus.CalculateDamageDealt();
                    enemy.TakeDamage(damage);

                    attacked = true;
                    yield return new WaitForSeconds(playerStatus.AttackDelay);
                    break; // 한 번 공격 후 다음 루프로
                }
            }

            // 공격할 대상이 없다면 루프 종료
            if (!attacked)
                break;
        }

        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (playerStatus == null) return;

        int reducedDamage = playerStatus.CalculateDamageTaken(damage);                

        if (playerStatus.IsDead)
        {
            Debug.Log("플레이어가 사망했습니다.");
        }
    }






    /// <summary>
    /// JSON 응답을 직접 적용 (좌표만 이동)
    /// target 이름 해석은 외부에서 처리됨
    /// </summary>
    public void ApplyActionJSON(string json)
    {
        var action = PlayerActionParser.Parse(json);
        if (action == null || !action.success)
        {
            Debug.LogWarning("유효하지 않은 명령입니다.");
            return;
        }

        Vector2 currentPos = transform.position;
        Vector2 targetPos = new Vector2(action.position.x, action.position.y);

        actionExecutor.Execute(action);

        Debug.Log($"플레이어 행동: {action.action} / {action.message}");
        // playerStatus.LogStatus();
    }

    /*
    /// <summary>
    /// 행동 종류에 따른 스테이터스 변화
    /// </summary>
    private void ApplyActionEffects(string actionType, Vector2 currentPos, Vector2 targetPos)
    {
        if (playerStatus == null) return;

        switch (actionType)
        {
            case "move":
                HandleMoveAction(currentPos, targetPos);
                break;

            case "rest":
                HandleRestAction();
                break;

            case "eat":
                HandleEatAction();
                break;

            case "attack":
                HandleAttackAction();
                break;

            default:
                HandleDefaultAction();
                break;
        }

        // 턴 진행 및 상태 확인
        playerStatus.NextTurn();
        CheckPlayerCondition();
    }

    /// <summary>
    /// 이동 행동 — 거리 기반 능력치 감소
    /// </summary>
    private void HandleMoveAction(Vector2 currentPos, Vector2 targetPos)
    {
        float distance = Vector2.Distance(currentPos, targetPos);

        int staminaLoss = Mathf.CeilToInt(distance * 3f);
        int hungerLoss = Mathf.CeilToInt(distance * 2f);

        playerStatus.ModifyStamina(-staminaLoss);
        playerStatus.ModifyHunger(-hungerLoss);

        Debug.Log($"[이동] 거리 {distance:F1} → 스태미나 -{staminaLoss}, 허기 -{hungerLoss}");
    }

    /// <summary>
    /// 휴식 행동 — 스태미나 회복, 허기 증가
    /// </summary>
    private void HandleRestAction()
    {
        playerStatus.ModifyStamina(+15);        
        Debug.Log("[휴식] 스태미나 +15, 허기 +5");
    }

    /// <summary>
    /// 음식 섭취 행동 — 허기 회복
    /// </summary>
    private void HandleEatAction()
    {
        playerStatus.ModifyHunger(+20);
        Debug.Log("[식사] 허기 +20");
    }

    /// <summary>
    /// 공격 행동 — 체력과 스태미나 감소
    /// </summary>
    private void HandleAttackAction()
    {
        playerStatus.ModifyStamina(-10);
        playerStatus.ModifyHealth(-5);
        Debug.Log("[공격] 스태미나 -10, 체력 -5");
    }

    /// <summary>
    /// 기본 행동 (기타)
    /// </summary>
    private void HandleDefaultAction()
    {
        playerStatus.ModifyStamina(-2);
        Debug.Log("[일반 행동] 스태미나 -2");
    }

    /// <summary>
    /// 생존 여부 확인
    /// </summary>
    private void CheckPlayerCondition()
    {
        if (playerStatus.IsDead)
            Debug.Log("게임 오버: 체력이 0이 되었습니다.");
        else if (playerStatus.IsExhausted)
            Debug.Log("게임 오버: 탈진 상태입니다.");
    }
    */


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerStatus.AttackRange);
    }
#endif
}