using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get;private set;}

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChange;
    public event EventHandler<OnClickOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickOnGridPositionEventArgs: EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle,
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypesArray;

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId==0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }
        
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback+= NetworkManager_OnClientConnectCallback;
        }
        currentPlayablePlayerType.OnValueChanged+=(PlayerType oldPlayerType,PlayerType newPlayerType)=>{OnCurrentPlayablePlayerTypeChange?.Invoke(this,EventArgs.Empty);};
    }

    private void NetworkManager_OnClientConnectCallback(ulong obj) 
    {
        if(NetworkManager.Singleton.ConnectedClientsList.Count ==2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }    
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("More than one GameManager instance detected!");
        }
        Instance = this;
        playerTypesArray = new PlayerType[3,3];
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc() 
    {
        OnGameStarted?.Invoke(this,EventArgs.Empty);
    }
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y,PlayerType playerType) 
    {
        Debug.Log($"Clicked On Grid Position {x},{y}");
        if(playerType !=currentPlayablePlayerType.Value)
        {
            return;
        }
        if(playerTypesArray[x,y]!=PlayerType.None)
        {
            return;
        }
        else
        {
            playerTypesArray[x,y] = playerType;
        }
        OnClickedOnGridPosition?.Invoke(this,new OnClickOnGridPositionEventArgs{x=x,y=y,playerType=playerType});  

        switch(currentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType.Value = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }
    }

    public PlayerType GetLocalPlayerType() 
    {
        return localPlayerType;    
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }
}
