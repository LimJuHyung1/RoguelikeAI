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

    [Header("플레이어 및 상태 데이터")]
    public Player player;

    private PlayerStatusSO playerStatus;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시에도 유지하려면 사용
        }
        else
        {
            Destroy(gameObject); // 중복 인스턴스 제거
        }        
    }

    void OnEnable()
    {
        if (player != null)
        {
            playerStatus = player.playerStatus;

            if (playerStatus != null)
            {
                // PlayerStatusSO 이벤트 구독
                playerStatus.OnStatusChanged += UpdateStats;
                playerStatus.OnPlayerDied += ShowDeathMessage;
                playerStatus.OnPlayerExhausted += ShowExhaustedMessage;
            }
        }

        // 초기 UI 표시
        UpdateStats();
        UpdateTurn();
    }

    void OnDisable()
    {
        if (playerStatus != null)
        {
            // 이벤트 구독 해제 (메모리 누수 방지)
            playerStatus.OnStatusChanged -= UpdateStats;
            playerStatus.OnPlayerDied -= ShowDeathMessage;
            playerStatus.OnPlayerExhausted -= ShowExhaustedMessage;
        }
    }



    public void SetPlayerStatus(PlayerStatusSO newStatus)
    {
        // 기존 이벤트 해제
        if (playerStatus != null)
        {
            playerStatus.OnStatusChanged -= UpdateStats;
            playerStatus.OnPlayerDied -= ShowDeathMessage;
            playerStatus.OnPlayerExhausted -= ShowExhaustedMessage;
        }

        // 새 상태 연결
        playerStatus = newStatus;

        if (playerStatus != null)
        {
            playerStatus.OnStatusChanged += UpdateStats;
            playerStatus.OnPlayerDied += ShowDeathMessage;
            playerStatus.OnPlayerExhausted += ShowExhaustedMessage;
        }

        // 즉시 UI 갱신
        UpdateStats();
        UpdateTurn();
    }




    // 상태 변경 시 자동 호출됨
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

    // 사망 상태
    private void ShowDeathMessage()
    {
        Debug.Log("플레이어가 사망했습니다!");
        hpText.text = "HP: 0 (Dead)";
    }

    // 탈진 상태
    private void ShowExhaustedMessage()
    {
        Debug.Log("플레이어가 탈진했습니다!");
        stmText.text = $"Stamina: {playerStatus.Stamina} (Exhausted)";
    }
}
