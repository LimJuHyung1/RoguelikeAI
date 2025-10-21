using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour
{
    public ObjectData data;
    private GameObject floatingNameObj;

    private void OnMouseEnter()
    {
        if (data == null) return;

        // 이름 표시 오브젝트 생성
        floatingNameObj = new GameObject("FloatingName");
        TextMesh textMesh = floatingNameObj.AddComponent<TextMesh>();
        textMesh.text = data.objectName;
        textMesh.fontSize = 60;
        textMesh.characterSize = 0.05f;
        textMesh.color = Color.white;
        textMesh.alignment = TextAlignment.Center;

        // 위치와 회전 설정
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
        // 카메라를 계속 바라보게
        if (floatingNameObj != null)
        {
            floatingNameObj.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
