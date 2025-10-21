using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectData", menuName = "Game Data/NewObjectData", order = 1)]
public class ObjectData : ScriptableObject
{
    public string objectName;
    [TextArea]
    public string description;
    public Sprite icon;

    // 확장 가능 필드 (예: 상호작용 타입, 점수, 효과 등)
    public bool isInteractable;
    public int value;
}
