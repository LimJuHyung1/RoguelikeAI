using UnityEngine;
using UnityEngine.UI;

public class FoodDataManager : MonoBehaviour
{
    public static FoodDataManager instance;

    [Header("��ϵ� ���� ������ ���")]
    public FoodDataSO[] allFoods; // Inspector���� ���

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// �̸����� FoodData �˻�
    /// </summary>
    public FoodDataSO FindFoodByName(string foodName)
    {
        foreach (var food in allFoods)
        {
            if (food != null && food.foodName == foodName)
                return food;
        }

        Debug.LogWarning($"[FoodDataManager] '{foodName}'�� �ش��ϴ� ���� �����͸� ã�� �� �����ϴ�.");
        return null;
    }
}
