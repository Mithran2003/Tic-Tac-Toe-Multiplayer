using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    [SerializeField] private Transform CrossPrefabTransform;
    [SerializeField] private Transform CirclePrefabTransform;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition+= GameManager_OnClickedOnGridPosition;
    }

    private void GameManager_OnClickedOnGridPosition(object sender,GameManager.OnClickOnGridPositionEventArgs eventArgs) 
    {
        Debug.Log("GameManager_OnClickedOnGridPosition");
        SpwanOnjectRpc(eventArgs.x,eventArgs.y);
    }
    [Rpc(SendTo.Server)]
    private void SpwanOnjectRpc(int x,int y) 
    {
        Debug.Log("SpwanObject");
        Transform SpwanedCrossPrebafTransform= Instantiate(CrossPrefabTransform,GetGridWorldPosition(x,y),Quaternion.identity);
        SpwanedCrossPrebafTransform.GetComponent<NetworkObject>().Spawn(true);
        
    }

    private Vector2 GetGridWorldPosition(int x,int y)
    {
        return new Vector2(x*GRID_SIZE,y*GRID_SIZE);
    }
}
