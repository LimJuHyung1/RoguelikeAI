using UnityEngine;
using UnityEngine.UI;

public class ObjectInfoManager : MonoBehaviour
{
    public static ObjectInfoManager Instance;

    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Text nameText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Image iconImage;

    private RectTransform panelRect;
    private Canvas canvas;

    private void Awake()
    {
        Instance = this;
        panelRect = infoPanel.GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        HideInfo();
    }

    private void Update()
    {
        if (infoPanel.activeSelf)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out pos
            );
            panelRect.anchoredPosition = pos + new Vector2(15f, -15f); // 커서 약간 옆에 표시
        }
    }

    public void ShowInfo(ObjectData data)
    {
        infoPanel.SetActive(true);
        nameText.text = data.objectName;
        descriptionText.text = data.description;

        if (iconImage != null && data.icon != null)
        {
            iconImage.sprite = data.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void HideInfo()
    {
        infoPanel.SetActive(false);
    }
}
