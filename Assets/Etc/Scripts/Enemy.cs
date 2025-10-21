using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    public TestAI testAI;  // TestAI 스크립트 참조
    public TestAI TestAI { get => testAI; set => testAI = value; }

    [Header("적 속성 데이터 (ObjectData)")]
    public ObjectData enemyData;        // 적의 이름, 설명, 속성 정보

    [Header("적 전투 데이터 (ScriptableObject)")]
    public EnemyStatusSO enemyStatus;   // 적의 전투 능력치 및 AI 설정

    private Player player;          // 플레이어 참조
    private bool isAttacking;
    private bool isWandering;
    private Coroutine moveCoroutine;
    private Coroutine wanderCoroutine;
    private Vector2 spawnPosition;

    [Header("데미지 텍스트 설정")]
    public Color damageTextColor = Color.red;
    public float damageTextHeight = 1.5f;
    public int damageFontSize = 70;
    public float damageCharSize = 0.1f;
    public float damageTextDuration = 1.2f;  // 표시 유지 시간



    void Start()
    {
        spawnPosition = transform.position;

        // 플레이어 참조 (Tag="Player" 오브젝트)
        player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (enemyStatus == null)
        {
            Debug.LogError($"{name}: EnemyStatusSO가 연결되어 있지 않습니다!");
            return;
        }

        enemyStatus = Instantiate(enemyStatus);

        // 초기 체력 설정
        enemyStatus.ResetStatus();

        Debug.Log($"HP: {enemyStatus.MaxHealth}, ATK: {enemyStatus.AttackPower}, DEF: {enemyStatus.DefensePower}");
    }


    void Update()
    {
        if (enemyStatus == null || enemyStatus.IsDead) return;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        // 공격 범위 내 → 공격 모드
        if (distance <= enemyStatus.AttackRange)
        {
            StopWandering();

            if (!isAttacking)
                StartCoroutine(AttackRoutine());
        }
        // 탐지 범위 내 → 추적 모드
        else if (distance <= enemyStatus.DetectionRange)
        {
            StopWandering();
            ChasePlayer();
        }
        // 탐지 범위 밖 → 어슬렁거리기
        else
        {
            if (!isWandering)
                wanderCoroutine = StartCoroutine(WanderRoutine());
        }
    }

    // ==========================
    // 추적(Chase) 관련 함수
    // ==========================
    private void ChasePlayer()
    {
        if (player == null || enemyStatus.IsDead) return;

        // 플레이어 방향 계산
        Vector2 dir = (player.transform.position - transform.position).normalized;

        // 천천히 플레이어 쪽으로 이동
        transform.position += (Vector3)(dir * enemyStatus.MoveSpeed * Time.deltaTime);

        // 방향 맞춰보기 (선택)
        transform.right = dir;
    }

    // -------------------------------
    // 어슬렁거리는 루틴 (탐지 범위 밖일 때)
    // -------------------------------
    private IEnumerator WanderRoutine()
    {
        isWandering = true;
        Debug.Log($"[{enemyData.objectName}]이(가) 어슬렁거리기 시작합니다.");

        while (true)
        {
            // EnemyStatusSO에서 값 가져오기
            float wanderRadius = enemyStatus.WanderRadius;
            float wanderMoveMin = enemyStatus.WanderMoveMin;
            float wanderMoveMax = enemyStatus.WanderMoveMax;
            float wanderIdleMin = enemyStatus.WanderIdleMin;
            float wanderIdleMax = enemyStatus.WanderIdleMax;

            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(wanderMoveMin, wanderMoveMax);
            Vector2 targetPos = (Vector2)transform.position + randomDir * randomDistance;

            Vector2 offsetFromSpawn = targetPos - spawnPosition;
            if (offsetFromSpawn.magnitude > wanderRadius)
                targetPos = spawnPosition + offsetFromSpawn.normalized * wanderRadius;

            yield return MoveToPoint(targetPos);

            float idleTime = Random.Range(wanderIdleMin, wanderIdleMax);
            yield return new WaitForSeconds(idleTime);

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= enemyStatus.DetectionRange)
            {
                Debug.Log($"[{enemyData.objectName}]이(가) 플레이어를 감지했습니다!");
                break;
            }
        }

        isWandering = false;
    }


    private void StopWandering()
    {
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
        isWandering = false;
    }



    private IEnumerator MoveToPoint(Vector2 target)
    {
        Vector2 start = transform.position;
        float distance = Vector2.Distance(start, target);
        float duration = distance / enemyStatus.MoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        transform.position = target;
    }


    /// <summary>
    /// 플레이어를 향해 이동
    /// </summary>
    private void MoveTowardPlayer()
    {
        if (moveCoroutine != null) return;
        moveCoroutine = StartCoroutine(MoveRoutine(player.transform.position));
    }

    private IEnumerator MoveRoutine(Vector2 target)
    {
        Vector2 start = transform.position;
        float distance = Vector2.Distance(start, target);
        float duration = distance / enemyStatus.MoveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        transform.position = target;
        moveCoroutine = null;
    }


    /// <summary>
    /// 공격 루틴 (공격 딜레이 적용)
    /// </summary>
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        yield return new WaitForSeconds(enemyStatus.AttackDelay);

        if (player == null || enemyStatus.IsDead)
        {
            isAttacking = false;
            yield break;
        }

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= enemyStatus.AttackRange)
        {
            AttackPlayer();
        }

        isAttacking = false;
    }


    /// <summary>
    /// 플레이어에게 피해 입히기
    /// </summary>
    private void AttackPlayer()
    {
        if (player == null || enemyStatus.IsDead) return;

        int damage = enemyStatus.CalculateAttackDamage();
        player.TakeDamage(damage); // 정상 작동

        Debug.Log($"[{enemyData.objectName}]이(가) 플레이어를 공격! 피해: {damage}");
    }



    /// <summary>
    /// 플레이어 공격으로부터 피해 받기
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (enemyStatus == null || enemyStatus.IsDead) return;

        int prevHP = enemyStatus.CurrentHealth;
        enemyStatus.TakeDamage(amount);
        int lostHP = Mathf.Clamp(prevHP - enemyStatus.CurrentHealth, 0, amount);

        Debug.Log($"[{enemyData.objectName}] 피해 입음 (남은 HP: {enemyStatus.CurrentHealth})");

        // 데미지 텍스트 표시
        if (lostHP > 0)
            ShowDamageText(lostHP);

        if (enemyStatus.IsDead)
            Die();
    }

    // 텍스트 표시 함수
    private void ShowDamageText(int damage)
    {
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(transform); // Enemy의 자식으로 설정
        TextMesh mesh = textObj.AddComponent<TextMesh>();

        mesh.text = "-" + damage.ToString();
        mesh.fontSize = damageFontSize;
        mesh.characterSize = damageCharSize;
        mesh.color = damageTextColor;
        mesh.alignment = TextAlignment.Center;

        textObj.transform.localPosition = Vector3.up * damageTextHeight; // 로컬 좌표 사용
        textObj.transform.rotation = Camera.main.transform.rotation;

        StartCoroutine(FadeAndDestroyText(mesh, textObj));
    }

    private IEnumerator FadeAndDestroyText(TextMesh mesh, GameObject obj)
    {
        Color c = mesh.color;
        float elapsed = 0f;

        while (elapsed < damageTextDuration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position += Vector3.up * Time.deltaTime * 0.5f; // 위로 살짝 이동
            c.a = Mathf.Lerp(1f, 0f, elapsed / damageTextDuration);
            mesh.color = c;
            yield return null;
        }

        Destroy(obj);
    }


    /// <summary>
    /// 적 사망 처리
    /// </summary>
    private void Die()
    {
        // testAI.DeleteSpot(this.name);    // 현재 오류 있음(적이 죽어서 참조 불가)
        Debug.Log($"[{enemyData.objectName}] 사망!");
        if (enemyData != null)
            Debug.Log($"플레이어가 {enemyData.value} 점을 얻었습니다!");
        StopAllCoroutines();
        Destroy(gameObject, 0.5f);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (enemyStatus == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyStatus.AttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyStatus.DetectionRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)spawnPosition : transform.position, enemyStatus.WanderRadius);
    }
#endif
}
