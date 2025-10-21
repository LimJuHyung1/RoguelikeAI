using UnityEngine;

public class GroundCoordinateDisplay : MonoBehaviour
{
    [Header("좌표 표시 설정")]
    public Color textColor = Color.yellow;
    public float textHeightOffset = 0.3f;
    public int fontSize = 60;
    public float characterSize = 0.05f;

    private GameObject coordinateTextObj;
    private TextMesh coordinateTextMesh;

    void Update()
    {
        // 마우스 포인터의 월드 좌표 직접 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPoint = new Vector2(mouseWorldPos.x, mouseWorldPos.y);


        // Raycast로 Ground만 감지 (2D Collider 기준)
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Ground"))
        {
            Vector2 point = hit.point;

            if (coordinateTextObj == null)
            {
                coordinateTextObj = new GameObject("GroundCoordinateText");
                coordinateTextMesh = coordinateTextObj.AddComponent<TextMesh>();
                coordinateTextMesh.fontSize = fontSize;
                coordinateTextMesh.characterSize = characterSize;
                coordinateTextMesh.color = textColor;
                coordinateTextMesh.anchor = TextAnchor.MiddleCenter;
                coordinateTextMesh.alignment = TextAlignment.Center;
            }

            coordinateTextObj.transform.position = new Vector3(point.x, point.y + textHeightOffset, 0);
            coordinateTextObj.transform.rotation = Camera.main.transform.rotation;

            // 절대 좌표를 표시 (플레이어 중심이 아닌 월드 좌표)
            coordinateTextMesh.text = $"({point.x:F1}, {point.y:F1})";
        }
        else
        {
            if (coordinateTextObj != null)
            {
                Destroy(coordinateTextObj);
                coordinateTextObj = null;
            }
        }
    }
}
