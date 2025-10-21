using UnityEngine;
using UnityEngine.UI;

public class FoodDataManager : MonoBehaviour
{
    public static FoodDataManager instance;

    [Header("등록된 음식 데이터 목록")]
    public FoodDataSO[] allFoods; // Inspector에서 등록

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// 이름으로 FoodData 검색
    /// </summary>
    public FoodDataSO FindFoodByName(string foodName)
    {
        foreach (var food in allFoods)
        {
            if (food != null && food.foodName == foodName)
                return food;
        }

        Debug.LogWarning($"[FoodDataManager] '{foodName}'에 해당하는 음식 데이터를 찾을 수 없습니다.");
        return null;
    }
}
