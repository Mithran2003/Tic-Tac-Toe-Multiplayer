using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour,IPointerDownHandler
{
    [SerializeField] private int X;
    [SerializeField] private int Y;
    public void OnPointerDown(PointerEventData eventData) 
    {
        Debug.Log($"Pointer Click on {X},{Y}");
        GameManager.Instance.ClickedOnGridPositionRpc(X,Y,GameManager.Instance.GetLocalPlayerType());    
    }
}
