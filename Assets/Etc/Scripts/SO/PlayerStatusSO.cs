using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "Game Data/Player Status", order = 0)]
public class PlayerStatusSO : ScriptableObject
{
    [Header("기본 스탯")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [Range(0, 100)][SerializeField] private int stamina = 100;
    [Range(0, 100)][SerializeField] private int hunger = 0;

    [Header("전투 스탯")]
    [SerializeField] private int attackPower = 10;     // 공격력
    [SerializeField] private int defensePower = 5;     // 방어력
    [Range(0f, 1f)][SerializeField] private float critChance = 0.1f;  // 치명타 확률 (10%)
    [Range(1f, 2f)][SerializeField] private float critMultiplier = 1.5f; // 치명타 배수
    [Range(0f, 1f)][SerializeField] private float evasionRate = 0.1f; // 회피 확률 (10%)

    [Header("턴 관련 설정")]
    [SerializeField] private int currentTurn = 0;
    [SerializeField] private int maxTurns = 24;

    [Header("전투 설정")]
    [SerializeField] private float attackRange = 5f;   // 공격 범위
    [SerializeField] private float attackDelay = 1.0f;   // 공격 딜레이

    [Header("감소량 / 회복량 설정")]
    [Tooltip("거리 1당 스태미나 소모량")]
    [SerializeField] private float staminaLossPerDistance = 2f;

    [Tooltip("거리 1당 허기 증가량")]
    [SerializeField] private float hungerLossPerDistance = 1.5f;

    [Tooltip("휴식 시 스태미나 회복량")]
    [SerializeField] private int restStaminaRecovery = 15;

    [Tooltip("휴식 시 허기 증가량")]
    [SerializeField] private int hungerPlusForRest = 10;

    [Tooltip("식사 시 허기 회복량")]
    [SerializeField] private int eatHungerRecovery = 20;

    [Header("상태 플래그")]
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isExhausted = false;
    [SerializeField] private bool survivedNight = false;

    // 이벤트 선언
    public event Action OnStatusChanged;       // 값이 바뀔 때마다 발생
    public event Action OnPlayerDied;          // 사망 시 호출
    public event Action OnPlayerExhausted;     // 탈진 시 호출
    public event Action OnNightSurvived;       // 밤 생존 시 호출


    // -----------------------------
    // 외부 접근용 프로퍼티 (읽기 전용)
    // -----------------------------
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public int Stamina => stamina;
    public int Hunger => hunger;
    public int AttackPower => attackPower;
    public int DefensePower => defensePower;
    public float CritChance => critChance;
    public float CritMultiplier => critMultiplier;
    public float EvasionRate => evasionRate;
    public float AttackRange => attackRange;
    public float AttackDelay => attackDelay;
    public int CurrentTurn => currentTurn;
    public int MaxTurns => maxTurns;
    public bool IsDead => isDead;
    public bool IsExhausted => isExhausted;
    public bool SurvivedNight => survivedNight;



    // ===================================================
    // 턴 기반 난이도 스케일 계산
    // ===================================================
    private float GetTurnScalingFactor()
    {
        // progress = 현재 턴 진행률 (0~1)
        float progress = Mathf.Clamp01((float)currentTurn / maxTurns);

        // 턴이 지날수록 1.0 → 2.0까지 배율 상승 (난이도 곡선)
        float scale = Mathf.Lerp(1f, 2f, progress);
        return scale;
    }

    // ===================================================
    // 이동 거리 기반 스탯 감소 (턴에 따라 점점 커짐)
    // ===================================================
    public void ApplyMovementCost(float distance)
    {
        float scale = GetTurnScalingFactor(); // 턴 기반 배율 적용

        int staminaLoss = Mathf.CeilToInt(distance * staminaLossPerDistance * scale);
        int hungerLoss = Mathf.CeilToInt(distance * hungerLossPerDistance * scale);

        ModifyStamina(-staminaLoss);
        ModifyHunger(hungerLoss);

        Debug.Log($"[이동 거리: {distance:F1}] 스태미나 -{staminaLoss}, 허기 +{hungerLoss} (배율 {scale:F2})");
    }

    // ===================================================
    public void ApplyRestEffect()
    {
        ModifyStamina(restStaminaRecovery);
        ModifyHunger(hungerPlusForRest);
        Debug.Log($"[휴식] 스태미나 +{restStaminaRecovery}, 허기 +{hungerPlusForRest}");
    }

    public void ApplyEatEffect(FoodDataSO food)
    {
        if (food == null)
        {
            Debug.LogWarning("음식 데이터가 없습니다!");
            return;
        }

        // 허기 수치 변화
        ModifyHunger(-food.hungerChange);

        // 체력 변화
        if (food.healthChange != 0)
            ModifyHealth(food.healthChange);

        // 로그 표시
        string direction = food.hungerChange >= 0 ? "감소" : "증가";
        Debug.Log($"[식사] {food.foodName} 섭취 → 허기 {direction} {Mathf.Abs(food.hungerChange)}, 체력 변화 {food.healthChange}");
    }






    // ===============================
    // 전투 계산 관련 메서드
    // ===============================
    public int CalculateDamageDealt()
    {
        bool isCritical = UnityEngine.Random.value < critChance;
        float finalDamage = attackPower * (isCritical ? critMultiplier : 1f);
        Debug.Log(isCritical ? $"치명타! {finalDamage} 피해" : $"공격! {finalDamage} 피해");
        return Mathf.RoundToInt(finalDamage);
    }

    public int CalculateDamageTaken(int incomingDamage)
    {
        bool evaded = UnityEngine.Random.value < evasionRate;
        if (evaded)
        {
            Debug.Log("공격 회피!");
            return 0;
        }

        int reducedDamage = Mathf.Max(0, incomingDamage - defensePower);
        ModifyHealth(-reducedDamage);
        Debug.Log($"방어 후 피해: {reducedDamage}");
        return reducedDamage;
    }






    // -----------------------------
    // 상태 조정 함수
    // -----------------------------
    public void ModifyHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnStatusChanged?.Invoke();

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            OnPlayerDied?.Invoke();
        }
    }

    public void ModifyStamina(int amount)
    {
        stamina = Mathf.Clamp(stamina + amount, 0, 100);
        OnStatusChanged?.Invoke();

        if (stamina <= 0 && !isExhausted)
        {
            isExhausted = true;
            OnPlayerExhausted?.Invoke();
        }
    }

    public void ModifyHunger(int amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0, 100);
        OnStatusChanged?.Invoke();
    }






    // -----------------------------
    // 턴 관리
    // -----------------------------
    public void NextTurn()
    {
        if (isDead || isExhausted || survivedNight) return;

        currentTurn++;
        OnStatusChanged?.Invoke();

        // GameEventManager에 턴 이벤트 전달
        GameEventManager.instance?.RaiseTurnPassed();

        if (currentTurn >= maxTurns)
        {
            survivedNight = true;
            OnNightSurvived?.Invoke();
        }

        StatManager.instance.UpdateStats();
        StatManager.instance.UpdateTurn();
    }






    // -----------------------------
    // 초기화
    // -----------------------------
    public void ResetStatus()
    {
        currentHealth = maxHealth;
        stamina = 100;
        hunger = 0;
        currentTurn = 0;
        isDead = false;
        isExhausted = false;
        survivedNight = false;
    }






    // -----------------------------
    // 상태 요약 출력
    // -----------------------------
    public void LogStatus()
    {
        // Debug.Log($"[턴 {currentTurn}/{maxTurns}] HP:{health} | STM:{stamina} | HUN:{hunger}");
    }
}
