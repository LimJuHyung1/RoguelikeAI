using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "PlayerStatus", menuName = "Game Data/Player Status", order = 0)]
public class PlayerStatusSO : ScriptableObject
{
    [Header("�⺻ ����")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;
    [Range(0, 100)][SerializeField] private int stamina = 100;
    [Range(0, 100)][SerializeField] private int hunger = 0;

    [Header("���� ����")]
    [SerializeField] private int attackPower = 10;     // ���ݷ�
    [SerializeField] private int defensePower = 5;     // ����
    [Range(0f, 1f)][SerializeField] private float critChance = 0.1f;  // ġ��Ÿ Ȯ�� (10%)
    [Range(1f, 2f)][SerializeField] private float critMultiplier = 1.5f; // ġ��Ÿ ���
    [Range(0f, 1f)][SerializeField] private float evasionRate = 0.1f; // ȸ�� Ȯ�� (10%)

    [Header("�� ���� ����")]
    [SerializeField] private int currentTurn = 0;
    [SerializeField] private int maxTurns = 24;

    [Header("���� ����")]
    [SerializeField] private float attackRange = 5f;   // ���� ����
    [SerializeField] private float attackDelay = 1.0f;   // ���� ������

    [Header("���ҷ� / ȸ���� ����")]
    [Tooltip("�Ÿ� 1�� ���¹̳� �Ҹ�")]
    [SerializeField] private float staminaLossPerDistance = 2f;

    [Tooltip("�Ÿ� 1�� ��� ������")]
    [SerializeField] private float hungerLossPerDistance = 1.5f;

    [Tooltip("�޽� �� ���¹̳� ȸ����")]
    [SerializeField] private int restStaminaRecovery = 15;

    [Tooltip("�޽� �� ��� ������")]
    [SerializeField] private int hungerPlusForRest = 10;

    [Tooltip("�Ļ� �� ��� ȸ����")]
    [SerializeField] private int eatHungerRecovery = 20;

    [Header("���� �÷���")]
    [SerializeField] private bool isDead = false;
    [SerializeField] private bool isExhausted = false;
    [SerializeField] private bool survivedNight = false;

    // �̺�Ʈ ����
    public event Action OnStatusChanged;       // ���� �ٲ� ������ �߻�
    public event Action OnPlayerDied;          // ��� �� ȣ��
    public event Action OnPlayerExhausted;     // Ż�� �� ȣ��
    public event Action OnNightSurvived;       // �� ���� �� ȣ��


    // -----------------------------
    // �ܺ� ���ٿ� ������Ƽ (�б� ����)
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
    // �� ��� ���̵� ������ ���
    // ===================================================
    private float GetTurnScalingFactor()
    {
        // progress = ���� �� ����� (0~1)
        float progress = Mathf.Clamp01((float)currentTurn / maxTurns);

        // ���� �������� 1.0 �� 2.0���� ���� ��� (���̵� �)
        float scale = Mathf.Lerp(1f, 2f, progress);
        return scale;
    }

    // ===================================================
    // �̵� �Ÿ� ��� ���� ���� (�Ͽ� ���� ���� Ŀ��)
    // ===================================================
    public void ApplyMovementCost(float distance)
    {
        float scale = GetTurnScalingFactor(); // �� ��� ���� ����

        int staminaLoss = Mathf.CeilToInt(distance * staminaLossPerDistance * scale);
        int hungerLoss = Mathf.CeilToInt(distance * hungerLossPerDistance * scale);

        ModifyStamina(-staminaLoss);
        ModifyHunger(hungerLoss);

        Debug.Log($"[�̵� �Ÿ�: {distance:F1}] ���¹̳� -{staminaLoss}, ��� +{hungerLoss} (���� {scale:F2})");
    }

    // ===================================================
    public void ApplyRestEffect()
    {
        ModifyStamina(restStaminaRecovery);
        ModifyHunger(hungerPlusForRest);
        Debug.Log($"[�޽�] ���¹̳� +{restStaminaRecovery}, ��� +{hungerPlusForRest}");
    }

    public void ApplyEatEffect(FoodDataSO food)
    {
        if (food == null)
        {
            Debug.LogWarning("���� �����Ͱ� �����ϴ�!");
            return;
        }

        // ��� ��ġ ��ȭ
        ModifyHunger(-food.hungerChange);

        // ü�� ��ȭ
        if (food.healthChange != 0)
            ModifyHealth(food.healthChange);

        // �α� ǥ��
        string direction = food.hungerChange >= 0 ? "����" : "����";
        Debug.Log($"[�Ļ�] {food.foodName} ���� �� ��� {direction} {Mathf.Abs(food.hungerChange)}, ü�� ��ȭ {food.healthChange}");
    }






    // ===============================
    // ���� ��� ���� �޼���
    // ===============================
    public int CalculateDamageDealt()
    {
        bool isCritical = UnityEngine.Random.value < critChance;
        float finalDamage = attackPower * (isCritical ? critMultiplier : 1f);
        Debug.Log(isCritical ? $"ġ��Ÿ! {finalDamage} ����" : $"����! {finalDamage} ����");
        return Mathf.RoundToInt(finalDamage);
    }

    public int CalculateDamageTaken(int incomingDamage)
    {
        bool evaded = UnityEngine.Random.value < evasionRate;
        if (evaded)
        {
            Debug.Log("���� ȸ��!");
            return 0;
        }

        int reducedDamage = Mathf.Max(0, incomingDamage - defensePower);
        ModifyHealth(-reducedDamage);
        Debug.Log($"��� �� ����: {reducedDamage}");
        return reducedDamage;
    }






    // -----------------------------
    // ���� ���� �Լ�
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
    // �� ����
    // -----------------------------
    public void NextTurn()
    {
        if (isDead || isExhausted || survivedNight) return;

        currentTurn++;
        OnStatusChanged?.Invoke();

        // GameEventManager�� �� �̺�Ʈ ����
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
    // �ʱ�ȭ
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
    // ���� ��� ���
    // -----------------------------
    public void LogStatus()
    {
        // Debug.Log($"[�� {currentTurn}/{maxTurns}] HP:{health} | STM:{stamina} | HUN:{hunger}");
    }
}
