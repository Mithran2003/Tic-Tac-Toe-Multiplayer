using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;
    [SerializeField] private Transform CrossPrefabTransform;
    [SerializeField] private Transform CirclePrefabTransform;
    [SerializeField] private Transform lineCompletePrefabTransform;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition+= GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin+=GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender,GameManager.OnGameWinEventArgs e) 
    {
        float eulerZ =0f;
        switch(e.line.orientation)
        {
            default:
                case GameManager.Orientation.Horizontal:
                    eulerZ=0f;
                    break;
                case GameManager.Orientation.Vertical:
                    eulerZ=90f;
                    break;
                case GameManager.Orientation.DigonalA:
                    eulerZ=45f;
                    break;
                case GameManager.Orientation.DigonalB:
                    eulerZ=-45f;
                    break;
        }
        Transform lineCompleteTransform= Instantiate(lineCompletePrefabTransform,GetGridWorldPosition(e.line.centerGridPosition.x,e.line.centerGridPosition.y),Quaternion.Euler(0,0,eulerZ));   
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true); 
    }

    private void GameManager_OnClickedOnGridPosition(object sender,GameManager.OnClickOnGridPositionEventArgs eventArgs) 
    {
        Debug.Log("GameManager_OnClickedOnGridPosition");
        SpwanOnjectRpc(eventArgs.x,eventArgs.y,eventArgs.playerType);
    }
    [Rpc(SendTo.Server)]
    private void SpwanOnjectRpc(int x,int y,GameManager.PlayerType playerType) 
    {
        Debug.Log("SpwanObject");
        Transform Prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                Prefab = CrossPrefabTransform;
                break;
            case GameManager.PlayerType.Circle:
                Prefab = CirclePrefabTransform;
                break;
        }
        Transform SpwanedCrossPrebafTransform= Instantiate(Prefab,GetGridWorldPosition(x,y),Quaternion.identity);
        SpwanedCrossPrebafTransform.GetComponent<NetworkObject>().Spawn(true);
        
    }

    private Vector2 GetGridWorldPosition(int x,int y)
    {
        return new Vector2(-GRID_SIZE+x*GRID_SIZE,-GRID_SIZE+y*GRID_SIZE);
    }
}
