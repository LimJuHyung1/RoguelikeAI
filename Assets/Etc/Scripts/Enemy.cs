using UnityEngine;
using System.Collections;
using JetBrains.Annotations;

[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    public TestAI testAI;  // TestAI ��ũ��Ʈ ����
    public TestAI TestAI { get => testAI; set => testAI = value; }

    [Header("�� �Ӽ� ������ (ObjectData)")]
    public ObjectData enemyData;        // ���� �̸�, ����, �Ӽ� ����

    [Header("�� ���� ������ (ScriptableObject)")]
    public EnemyStatusSO enemyStatus;   // ���� ���� �ɷ�ġ �� AI ����

    private Player player;          // �÷��̾� ����
    private bool isAttacking;
    private bool isWandering;
    private Coroutine moveCoroutine;
    private Coroutine wanderCoroutine;
    private Vector2 spawnPosition;

    [Header("������ �ؽ�Ʈ ����")]
    public Color damageTextColor = Color.red;
    public float damageTextHeight = 1.5f;
    public int damageFontSize = 70;
    public float damageCharSize = 0.1f;
    public float damageTextDuration = 1.2f;  // ǥ�� ���� �ð�



    void Start()
    {
        spawnPosition = transform.position;

        // �÷��̾� ���� (Tag="Player" ������Ʈ)
        player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (enemyStatus == null)
        {
            Debug.LogError($"{name}: EnemyStatusSO�� ����Ǿ� ���� �ʽ��ϴ�!");
            return;
        }

        enemyStatus = Instantiate(enemyStatus);

        // �ʱ� ü�� ����
        enemyStatus.ResetStatus();

        Debug.Log($"HP: {enemyStatus.MaxHealth}, ATK: {enemyStatus.AttackPower}, DEF: {enemyStatus.DefensePower}");
    }


    void Update()
    {
        if (enemyStatus == null || enemyStatus.IsDead) return;
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        // ���� ���� �� �� ���� ���
        if (distance <= enemyStatus.AttackRange)
        {
            StopWandering();

            if (!isAttacking)
                StartCoroutine(AttackRoutine());
        }
        // Ž�� ���� �� �� ���� ���
        else if (distance <= enemyStatus.DetectionRange)
        {
            StopWandering();
            ChasePlayer();
        }
        // Ž�� ���� �� �� ����Ÿ���
        else
        {
            if (!isWandering)
                wanderCoroutine = StartCoroutine(WanderRoutine());
        }
    }

    // ==========================
    // ����(Chase) ���� �Լ�
    // ==========================
    private void ChasePlayer()
    {
        if (player == null || enemyStatus.IsDead) return;

        // �÷��̾� ���� ���
        Vector2 dir = (player.transform.position - transform.position).normalized;

        // õõ�� �÷��̾� ������ �̵�
        transform.position += (Vector3)(dir * enemyStatus.MoveSpeed * Time.deltaTime);

        // ���� ���纸�� (����)
        transform.right = dir;
    }

    // -------------------------------
    // ����Ÿ��� ��ƾ (Ž�� ���� ���� ��)
    // -------------------------------
    private IEnumerator WanderRoutine()
    {
        isWandering = true;
        Debug.Log($"[{enemyData.objectName}]��(��) ����Ÿ��� �����մϴ�.");

        while (true)
        {
            // EnemyStatusSO���� �� ��������
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
                Debug.Log($"[{enemyData.objectName}]��(��) �÷��̾ �����߽��ϴ�!");
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
    /// �÷��̾ ���� �̵�
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
    /// ���� ��ƾ (���� ������ ����)
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
    /// �÷��̾�� ���� ������
    /// </summary>
    private void AttackPlayer()
    {
        if (player == null || enemyStatus.IsDead) return;

        int damage = enemyStatus.CalculateAttackDamage();
        player.TakeDamage(damage); // ���� �۵�

        Debug.Log($"[{enemyData.objectName}]��(��) �÷��̾ ����! ����: {damage}");
    }



    /// <summary>
    /// �÷��̾� �������κ��� ���� �ޱ�
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (enemyStatus == null || enemyStatus.IsDead) return;

        int prevHP = enemyStatus.CurrentHealth;
        enemyStatus.TakeDamage(amount);
        int lostHP = Mathf.Clamp(prevHP - enemyStatus.CurrentHealth, 0, amount);

        Debug.Log($"[{enemyData.objectName}] ���� ���� (���� HP: {enemyStatus.CurrentHealth})");

        // ������ �ؽ�Ʈ ǥ��
        if (lostHP > 0)
            ShowDamageText(lostHP);

        if (enemyStatus.IsDead)
            Die();
    }

    // �ؽ�Ʈ ǥ�� �Լ�
    private void ShowDamageText(int damage)
    {
        GameObject textObj = new GameObject("DamageText");
        textObj.transform.SetParent(transform); // Enemy�� �ڽ����� ����
        TextMesh mesh = textObj.AddComponent<TextMesh>();

        mesh.text = "-" + damage.ToString();
        mesh.fontSize = damageFontSize;
        mesh.characterSize = damageCharSize;
        mesh.color = damageTextColor;
        mesh.alignment = TextAlignment.Center;

        textObj.transform.localPosition = Vector3.up * damageTextHeight; // ���� ��ǥ ���
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
            obj.transform.position += Vector3.up * Time.deltaTime * 0.5f; // ���� ��¦ �̵�
            c.a = Mathf.Lerp(1f, 0f, elapsed / damageTextDuration);
            mesh.color = c;
            yield return null;
        }

        Destroy(obj);
    }


    /// <summary>
    /// �� ��� ó��
    /// </summary>
    private void Die()
    {
        // testAI.DeleteSpot(this.name);    // ���� ���� ����(���� �׾ ���� �Ұ�)
        Debug.Log($"[{enemyData.objectName}] ���!");
        if (enemyData != null)
            Debug.Log($"�÷��̾ {enemyData.value} ���� ������ϴ�!");
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
