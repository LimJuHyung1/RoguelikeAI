using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player;
    private Player playerScript; // Player 스크립트 참조

    [Header("Offset & Follow")]
    public Vector3 offset = new Vector3(0, 0, -10);
    public float followSpeed = 5f;
    public float dragSpeed = 0.1f;
    public float returnDelay = 2f;

    [Header("Camera Bounds (Dynamic)")]
    public bool useDynamicBounds = true;
    public float boundXRange = 5f; // 플레이어 기준 X 이동 가능 거리
    public float boundYRange = 3f; // 플레이어 기준 Y 이동 가능 거리

    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private float idleTimer = 0f;
    private bool isReturning = false;

    // 내부 계산용 (최종 카메라 제한)
    private float minX, maxX, minY, maxY;





    void Start()
    {
        if (player != null)
            playerScript = player.GetComponent<Player>();
    }

    void LateUpdate()
    {
        if (player == null) return;
        UpdateDynamicBounds(); // 플레이어 currentPosition 기준으로 범위 갱신
        HandleMouseDrag();
    }

    // 플레이어 위치 기준으로 카메라 이동 범위 갱신
    private void UpdateDynamicBounds()
    {
        if (playerScript == null) return;

        Vector2 center = playerScript.CurrentPosition; // Player의 현재 위치
        minX = center.x - boundXRange;
        maxX = center.x + boundXRange;
        minY = center.y - boundYRange;
        maxY = center.y + boundYRange;
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            isReturning = false;
            idleTimer = 0f;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            idleTimer = 0f;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x * dragSpeed, -delta.y * dragSpeed, 0);
            transform.position += move;

            if (useDynamicBounds)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(transform.position.x, minX, maxX),
                    Mathf.Clamp(transform.position.y, minY, maxY),
                    transform.position.z
                );
            }

            lastMousePosition = Input.mousePosition;
        }
        else
        {
            if (!isReturning)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= returnDelay)
                    isReturning = true;
            }
            else
            {
                Vector3 targetPos = player.position + offset;
                Vector3 newPos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

                if (useDynamicBounds)
                {
                    newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
                    newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
                }

                transform.position = newPos;
            }
        }
    }



#if UNITY_EDITOR
    // Scene 뷰에서 카메라 제한 범위 시각화
    private void OnDrawGizmosSelected()
    {
        if (!useDynamicBounds || playerScript == null) return;

        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3(playerScript.CurrentPosition.x, playerScript.CurrentPosition.y, 0);
        Vector3 size = new Vector3(boundXRange * 2f, boundYRange * 2f, 0);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
