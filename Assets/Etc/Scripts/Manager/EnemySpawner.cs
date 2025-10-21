using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    public TestAI testAI;

    [Header("������ �� ������")]
    public GameObject enemyPrefab;

    [Header("���� ���� ����")]
    public float spawnRadiusMin = 10f;   // �÷��̾� �ֺ� �ּ� �Ÿ�
    public float spawnRadiusMax = 20f;  // �÷��̾� �ֺ� �ִ� �Ÿ�

    [Header("�� ���� ���̵� ����")]
    public int baseSpawnCount = 1;      // �ʹ� �� �⺻ ���� ��
    public float spawnGrowthRate = 0.5f; // �ϴ� ���� ���� (0.5 �� 2�ϴ� +1)

    private Transform player;
    private PlayerStatusSO playerStatus;

    private void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        playerStatus = player?.GetComponent<Player>()?.playerStatus;

        // ���� �̺�Ʈ ����
        if (GameEventManager.instance != null)
            GameEventManager.instance.OnTurnPassed += SpawnEnemyOnTurn;
        else
            Debug.LogError("[EnemySpawner] GameEventManager �ν��Ͻ��� ã�� �� �����ϴ�.");
    }

    private void OnDestroy()
    {
        if (GameEventManager.instance != null)
            GameEventManager.instance.OnTurnPassed -= SpawnEnemyOnTurn;
    }

    // ���� ���� ������ Enemy ����
    private void SpawnEnemyOnTurn()
    {
        if (enemyPrefab == null || player == null) return;

        int currentTurn = GameEventManager.instance != null
            ? FindObjectOfType<Player>()?.playerStatus.CurrentTurn ?? 0
            : 0;

        int spawnCount = Mathf.FloorToInt(baseSpawnCount + (currentTurn * spawnGrowthRate));
        Debug.Log($"[�� ����] ���� �� {currentTurn}, ������ �� ��: {spawnCount}");

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
