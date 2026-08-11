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
    private PlayerType currentPlayablePlayerType;

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
    }

    private void NetworkManager_OnClientConnectCallback(ulong obj) 
    {
        if(NetworkManager.Singleton.ConnectedClientsList.Count ==2)
        {
            currentPlayablePlayerType = PlayerType.Cross;
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
        if(playerType !=currentPlayablePlayerType)
        {
            return;
        }
        OnClickedOnGridPosition?.Invoke(this,new OnClickOnGridPositionEventArgs{x=x,y=y,playerType=playerType});  

        switch(currentPlayablePlayerType)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType = PlayerType.Cross;
                break;
        }
        TriggerOnCurrentPlayablePlayerTypeChangeRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnCurrentPlayablePlayerTypeChangeRpc() 
    {
        OnCurrentPlayablePlayerTypeChange?.Invoke(this,EventArgs.Empty);
    }

    public PlayerType GetLocalPlayerType() 
    {
        return localPlayerType;    
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType;
    }
}
