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

    // 턴이 지날 때 호출되는 함수
    public void RaiseTurnPassed()
    {
        OnTurnPassed?.Invoke();
    }
}
