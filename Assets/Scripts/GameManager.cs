using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.LogFormat("Clicked on grid position {0}, {1}", x, y);
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y
        });
    }
}
