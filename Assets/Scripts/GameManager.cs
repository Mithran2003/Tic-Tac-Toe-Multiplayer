using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance {get;private set;}

    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }
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

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DigonalA,
        DigonalB,
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] playerTypesArray;
    private List<Line> linesList;

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

        linesList = new List<Line>
        { //HorizontalLines
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0)},
                centerGridPosition = new Vector2Int(1,0),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(2,1)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Horizontal,
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(1,2),
                orientation = Orientation.Horizontal,
            },
            //VerticalLines
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0),new Vector2Int(0,1),new Vector2Int(0,2)},
                centerGridPosition = new Vector2Int(0,1),
                orientation = Orientation.Vertical,
            },new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(1,0),new Vector2Int(1,1),new Vector2Int(1,2)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Vertical,
            },new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(2,0),new Vector2Int(2,1),new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(2,1),
                orientation = Orientation.Vertical,
            },
            //DiagnoalA
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,0),new Vector2Int(1,1),new Vector2Int(2,2)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DigonalA,
            },
            //DiagnoalB
            new Line
            {
                gridVector2IntList = new List<Vector2Int>{new Vector2Int(0,2),new Vector2Int(1,1),new Vector2Int(2,0)},
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DigonalB,
            },
        };
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
        TestWinner();
    }

    public PlayerType GetLocalPlayerType() 
    {
        return localPlayerType;    
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(playerTypesArray[line.gridVector2IntList[0].x,line.gridVector2IntList[0].y],
        playerTypesArray[line.gridVector2IntList[1].x,line.gridVector2IntList[1].y],
        playerTypesArray[line.gridVector2IntList[2].x,line.gridVector2IntList[2].y]);
    }
    private bool TestWinnerLine(PlayerType aPlayerType,PlayerType bPlayerType,PlayerType cPlayerType)
    {
        return
        aPlayerType!= PlayerType.None &&
        aPlayerType==bPlayerType&&
        bPlayerType==cPlayerType;
    }

    private void TestWinner() 
    {
        foreach (Line line in linesList)
        {
            if(TestWinnerLine(line))
            {
                //winner!
                Debug.Log("Winner,Game Over!");
                currentPlayablePlayerType.Value = PlayerType.None;
                OnGameWin?.Invoke(this,new OnGameWinEventArgs{line=line});
                break;
            }
        }
        
    }
}
