using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public TestAI testAI;

    [Header("생성할 적 프리팹")]
    public GameObject enemyPrefab;

    [Header("스폰 범위 설정")]
    public float spawnRadiusMin = 10f;   // 플레이어 주변 최소 거리
    public float spawnRadiusMax = 20f;  // 플레이어 주변 최대 거리

    [Header("턴 진행 난이도 설정")]
    public int baseSpawnCount = 1;      // 초반 턴 기본 생성 수
    public float spawnGrowthRate = 0.5f; // 턴당 증가 비율 (0.5 → 2턴당 +1)

    private Transform player;
    private PlayerStatusSO playerStatus;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        playerStatus = player?.GetComponent<Player>()?.playerStatus;

        // 전역 이벤트 구독
        if (GameEventManager.instance != null)
            GameEventManager.instance.OnTurnPassed += SpawnEnemyOnTurn;
        else
            Debug.LogError("[EnemySpawner] GameEventManager 인스턴스를 찾을 수 없습니다.");
    }

    private void OnDestroy()
    {
        if (GameEventManager.instance != null)
            GameEventManager.instance.OnTurnPassed -= SpawnEnemyOnTurn;
    }

    // 턴이 지날 때마다 Enemy 생성
    private void SpawnEnemyOnTurn()
    {
        if (enemyPrefab == null || player == null) return;

        int currentTurn = GameEventManager.instance != null
            ? FindObjectOfType<Player>()?.playerStatus.CurrentTurn ?? 0
            : 0;

        int spawnCount = Mathf.FloorToInt(baseSpawnCount + (currentTurn * spawnGrowthRate));
        Debug.Log($"[턴 스폰] 현재 턴 {currentTurn}, 생성할 적 수: {spawnCount}");

        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomDist = Random.Range(spawnRadiusMin, spawnRadiusMax);
            Vector3 spawnPos = player.position + new Vector3(randomDir.x, randomDir.y, 0) * randomDist;

            GameObject mob = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            if (testAI != null)
            {
                Enemy enemy = mob.GetComponent<Enemy>();
                enemy.TestAI = testAI;
                testAI.AddSpot(mob.transform);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        if (player == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, spawnRadiusMin);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, spawnRadiusMax);
    }
#endif
}
