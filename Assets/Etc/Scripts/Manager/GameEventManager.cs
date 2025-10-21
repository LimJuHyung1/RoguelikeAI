using System;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager instance;

    public event Action OnTurnPassed;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ���� ���� �� ȣ��Ǵ� �Լ�
    public void RaiseTurnPassed()
    {
        OnTurnPassed?.Invoke();
    }
}
