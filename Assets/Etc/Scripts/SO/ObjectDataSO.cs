using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectData", menuName = "Game Data/NewObjectData", order = 1)]
public class ObjectData : ScriptableObject
{
    public string objectName;
    [TextArea]
    public string description;
    public Sprite icon;

    // Ȯ�� ���� �ʵ� (��: ��ȣ�ۿ� Ÿ��, ����, ȿ�� ��)
    public bool isInteractable;
    public int value;
}
