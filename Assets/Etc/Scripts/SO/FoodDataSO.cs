using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodData", menuName = "Game Data/Food Data", order = 1)]
public class FoodDataSO : ScriptableObject
{
    [Header("���� �⺻ ����")]
    public string foodName;
    [TextArea] public string description;

    [Header("���� ��ȭ��")]
    public int hungerChange;    // ��� ��ȭ�� (���: ȸ��, ����: ��� ����)
    public int healthChange;    // ü�� ��ȭ�� (���� ���� ��� ��� ����)

    [Header("��Ÿ ����")]
    public Sprite icon;         // UI ������
    public AudioClip eatSound;  // ���� ȿ���� (����)

    [Header("����")]
    public bool isSpoiled;      // ���� ���� ����
}
