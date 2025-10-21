using UnityEngine;
using UnityEngine.UI;

public class StatManager : MonoBehaviour
{
    public static StatManager instance;

    [Header("UI Elements")]
    public Text hpText;
    public Text stmText;
    public Text hunText;
    public Text turnText;

    [Header("�÷��̾� �� ���� ������")]
    public Player player;

    private PlayerStatusSO playerStatus;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �ÿ��� �����Ϸ��� ���
        }
        else
        {
            Destroy(gameObject); // �ߺ� �ν��Ͻ� ����
        }        
    }

    void OnEnable()
    {
        if (player != null)
        {
            playerStatus = player.playerStatus;

            if (playerStatus != null)
            {
                // PlayerStatusSO �̺�Ʈ ����
                playerStatus.OnStatusChanged += UpdateStats;
                playerStatus.OnPlayerDied += ShowDeathMessage;
                playerStatus.OnPlayerExhausted += ShowExhaustedMessage;
            }
        }

        // �ʱ� UI ǥ��
        UpdateStats();
        UpdateTurn();
    }

    void OnDisable()
    {
        if (playerStatus != null)
        {
            // �̺�Ʈ ���� ���� (�޸� ���� ����)
            playerStatus.OnStatusChanged -= UpdateStats;
            playerStatus.OnPlayerDied -= ShowDeathMessage;
            playerStatus.OnPlayerExhausted -= ShowExhaustedMessage;
        }
    }



    public void SetPlayerStatus(PlayerStatusSO newStatus)
    {
        // ���� �̺�Ʈ ����
        if (playerStatus != null)
        {
            playerStatus.OnStatusChanged -= UpdateStats;
            playerStatus.OnPlayerDied -= ShowDeathMessage;
            playerStatus.OnPlayerExhausted -= ShowExhaustedMessage;
        }

        // �� ���� ����
        playerStatus = newStatus;

        if (playerStatus != null)
        {
            playerStatus.OnStatusChanged += UpdateStats;
            playerStatus.OnPlayerDied += ShowDeathMessage;
            playerStatus.OnPlayerExhausted += ShowExhaustedMessage;
        }

        // ��� UI ����
        UpdateStats();
        UpdateTurn();
    }




    // ���� ���� �� �ڵ� ȣ���
    public void UpdateStats()
    {
        if (playerStatus == null) return;

        hpText.text = $"HP: {playerStatus.CurrentHealth}";
        stmText.text = $"Stamina: {playerStatus.Stamina}";
        hunText.text = $"Hunger: {playerStatus.Hunger}";
    }

    public void UpdateTurn()
    {
        if (playerStatus == null) return;
        turnText.text = $"Turn: {playerStatus.CurrentTurn} / {playerStatus.MaxTurns}";
    }

    // ��� ����
    private void ShowDeathMessage()
    {
        Debug.Log("�÷��̾ ����߽��ϴ�!");
        hpText.text = "HP: 0 (Dead)";
    }

    // Ż�� ����
    private void ShowExhaustedMessage()
    {
        Debug.Log("�÷��̾ Ż���߽��ϴ�!");
        stmText.text = $"Stamina: {playerStatus.Stamina} (Exhausted)";
    }
}
