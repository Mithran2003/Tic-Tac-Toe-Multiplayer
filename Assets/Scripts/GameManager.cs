using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {get;private set;}

    public event EventHandler<OnClickOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickOnGridPositionEventArgs: EventArgs
    {
        public int x;
        public int y;
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("More than one GameManager instance detected!");
        }
        Instance = this;
    }
    public void ClickedOnGridPosition(int x, int y) 
    {
        Debug.Log($"Clicked On Grid Position {x},{y}");
        OnClickedOnGridPosition?.Invoke(this,new OnClickOnGridPositionEventArgs{x=x,y=y});    
    }
}
