using UnityEngine;

public class GroundCoordinateDisplay : MonoBehaviour
{
    [Header("��ǥ ǥ�� ����")]
    public Color textColor = Color.yellow;
    public float textHeightOffset = 0.3f;
    public int fontSize = 60;
    public float characterSize = 0.05f;

    private GameObject coordinateTextObj;
    private TextMesh coordinateTextMesh;

    void Update()
    {
        // ���콺 �������� ���� ��ǥ ���� ��ȯ
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 worldPoint = new Vector2(mouseWorldPos.x, mouseWorldPos.y);


        // Raycast�� Ground�� ���� (2D Collider ����)
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

            // ���� ��ǥ�� ǥ�� (�÷��̾� �߽��� �ƴ� ���� ��ǥ)
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
