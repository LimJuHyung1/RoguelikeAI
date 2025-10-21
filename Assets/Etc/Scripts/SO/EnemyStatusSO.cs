using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "Game Data/Enemy Status", order = 1)]
public class EnemyStatusSO : ScriptableObject
{
    [Header("�⺻ ����")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackPower = 10;
    [SerializeField] private int defensePower = 3;

    [Header("���� Ȯ��")]
    [Range(0f, 1f)]
    [SerializeField] private float critChance = 0.1f;       // ġ��Ÿ Ȯ�� (10%)
    [Range(1f, 2f)]
    [SerializeField] private float critMultiplier = 1.5f;   // ġ��Ÿ ���
    [Range(0f, 1f)]
    [SerializeField] private float evasionRate = 0.05f;     // ȸ�� Ȯ�� (5%)

    [Header("AI �ൿ ����")]
    [SerializeField] private float detectionRange = 3f;     // �÷��̾� ���� �Ÿ�
    [SerializeField] private float attackRange = 1.5f;      // ���� ���� �Ÿ�
    [SerializeField] private float attackDelay = 1.5f;      // ���� ����
    [SerializeField] private float moveSpeed = 2f;          // �̵� �ӵ�

    [Header("����Ÿ� ����")]
    [SerializeField] private float wanderRadius = 1f;       // ����Ÿ� �� �ִ� �ִ� �ݰ�
    [SerializeField] private float wanderMoveMin = 1f;      // �ּ� �̵� �Ÿ�
    [SerializeField] private float wanderMoveMax = 1f;      // �ִ� �̵� �Ÿ�
    [SerializeField] private float wanderIdleMin = 1f;      // �ּ� ��� �ð�
    [SerializeField] private float wanderIdleMax = 3f;      // �ִ� ��� �ð�

    // �ܺ� ���ٿ� ������Ƽ
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int AttackPower => attackPower;
    public int DefensePower => defensePower;
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;
    public float EvasionRate => evasionRate;
    public float DetectionRange => detectionRange;
    public float AttackRange => attackRange;
    public float AttackDelay => attackDelay;
    public float MoveSpeed => moveSpeed;
    public float WanderRadius => wanderRadius;
    public float WanderMoveMin => wanderMoveMin;
    public float WanderMoveMax => wanderMoveMax;
    public float WanderIdleMin => wanderIdleMin;
    public float WanderIdleMax => wanderIdleMax;

    public bool IsDead => currentHealth <= 0;



    // ======================
    // ���� ���� �Լ�
    // ======================
    public void ResetStatus()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        bool evaded = Random.value < evasionRate;
        if (evaded)
        {
            Debug.Log("���� ������ ȸ���߽��ϴ�!");
            return;
        }

        int finalDamage = Mathf.Max(0, damage - defensePower);
        currentHealth = Mathf.Clamp(currentHealth - finalDamage, 0, maxHealth);
    }

    public int CalculateAttackDamage()
    {
        bool isCritical = Random.value < critChance;
        float multiplier = isCritical ? critMultiplier : 1f;
        int finalDamage = Mathf.RoundToInt(attackPower * multiplier);

        if (isCritical)
            Debug.Log($"ġ��Ÿ ����! {finalDamage} ����");

        return finalDamage;
    }
}
