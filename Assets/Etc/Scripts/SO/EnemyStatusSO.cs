using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStatus", menuName = "Game Data/Enemy Status", order = 1)]
public class EnemyStatusSO : ScriptableObject
{
    [Header("기본 스탯")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private int attackPower = 10;
    [SerializeField] private int defensePower = 3;

    [Header("전투 확률")]
    [Range(0f, 1f)]
    [SerializeField] private float critChance = 0.1f;       // 치명타 확률 (10%)
    [Range(1f, 2f)]
    [SerializeField] private float critMultiplier = 1.5f;   // 치명타 배수
    [Range(0f, 1f)]
    [SerializeField] private float evasionRate = 0.05f;     // 회피 확률 (5%)

    [Header("AI 행동 설정")]
    [SerializeField] private float detectionRange = 3f;     // 플레이어 감지 거리
    [SerializeField] private float attackRange = 1.5f;      // 공격 가능 거리
    [SerializeField] private float attackDelay = 1.5f;      // 공격 간격
    [SerializeField] private float moveSpeed = 2f;          // 이동 속도

    [Header("어슬렁거림 설정")]
    [SerializeField] private float wanderRadius = 1f;       // 어슬렁거릴 수 있는 최대 반경
    [SerializeField] private float wanderMoveMin = 1f;      // 최소 이동 거리
    [SerializeField] private float wanderMoveMax = 1f;      // 최대 이동 거리
    [SerializeField] private float wanderIdleMin = 1f;      // 최소 대기 시간
    [SerializeField] private float wanderIdleMax = 3f;      // 최대 대기 시간

    // 외부 접근용 프로퍼티
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
    // 상태 관리 함수
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
            Debug.Log("적이 공격을 회피했습니다!");
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
            Debug.Log($"치명타 공격! {finalDamage} 피해");

        return finalDamage;
    }
}
