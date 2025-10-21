using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour
{
    public ObjectData data;
    private GameObject floatingNameObj;

    private void OnMouseEnter()
    {
        if (data == null) return;

        // �̸� ǥ�� ������Ʈ ����
        floatingNameObj = new GameObject("FloatingName");
        TextMesh textMesh = floatingNameObj.AddComponent<TextMesh>();
        textMesh.text = data.objectName;
        textMesh.fontSize = 60;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignment.Center;

        // ��ġ�� ȸ�� ����
        floatingNameObj.transform.position = transform.position + Vector3.up * 1.5f;
        floatingNameObj.transform.rotation = Camera.main.transform.rotation;
    }

    private void OnMouseExit()
    {
        if (floatingNameObj != null)
        {
            Destroy(floatingNameObj);
        }
    }

    private void Update()
    {
        // ī�޶� ��� �ٶ󺸰�
        if (floatingNameObj != null)
        {
            floatingNameObj.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
