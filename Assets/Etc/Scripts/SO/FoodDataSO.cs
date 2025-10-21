using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodData", menuName = "Game Data/Food Data", order = 1)]
public class FoodDataSO : ScriptableObject
{
    [Header("음식 기본 정보")]
    public string foodName;
    [TextArea] public string description;

    [Header("스탯 변화량")]
    public int hungerChange;    // 허기 변화량 (양수: 회복, 음수: 허기 증가)
    public int healthChange;    // 체력 변화량 (썩은 음식 등에서 사용 가능)

    [Header("기타 설정")]
    public Sprite icon;         // UI 아이콘
    public AudioClip eatSound;  // 섭취 효과음 (선택)

    [Header("상태")]
    public bool isSpoiled;      // 썩은 음식 여부
}
