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
        public PlayerType winPlayerType;
    }
    public event EventHandler OnGameTied;
    public event EventHandler OnRematch;
    public event EventHandler OnScoreChanged;
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
    private NetworkVariable<int> playerCrossScore= new NetworkVariable<int>();
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>();

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

        playerCrossScore.OnValueChanged+=(int prevScore,int newScore)=>
        {
            OnScoreChanged?.Invoke(this,EventArgs.Empty);
        };
        playerCircleScore.OnValueChanged+=(int prevScore,int newScore)=>
        {
            OnScoreChanged?.Invoke(this,EventArgs.Empty);
        };
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
        for (int i=0;i<linesList.Count;i++)
        {
            Line line = linesList[i];
            if(TestWinnerLine(line))
            {
                //winner!
                Debug.Log("Winner,Game Over!");
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType=playerTypesArray[line.centerGridPosition.x,line.centerGridPosition.y];
                
                switch(winPlayerType)
                {
                    case PlayerType.Cross:
                        playerCrossScore.Value++;
                        break;
                    case PlayerType.Circle:
                        playerCircleScore.Value++;
                        break;  
                }
                TriggerOnGameWinRpc(i,winPlayerType);
                break;
            }
        }
        bool hasTie = true;
        for (int x=0;x<playerTypesArray.GetLength(0);x++)
        {
            for(int y=0;y<playerTypesArray.GetLength(0);y++)
            {
                if(playerTypesArray[x,y]==PlayerType.None)
                {
                    hasTie=false;
                    break;
                }
            }
        }
        if(hasTie)
        {
            TriggerOnTiedRpc();
        }
        
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnTiedRpc() 
    {
        OnGameTied?.Invoke(this,EventArgs.Empty);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex,PlayerType winPlayerType) 
    {
        Line line=linesList[lineIndex];
        OnGameWin?.Invoke(this,new OnGameWinEventArgs{line=line,winPlayerType=winPlayerType});
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc() 
    {
        for(int x = 0; x < playerTypesArray.GetLength(0); x++) 
        {
            for(int y = 0; y <playerTypesArray.GetLength(0) ; y++) 
            {
                playerTypesArray[x,y]=PlayerType.None;
            } 
        }
        currentPlayablePlayerType.Value=PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc() 
    {
        OnRematch?.Invoke(this,EventArgs.Empty);
    }

    public void GetScore(out int playerCrossScore,out int playerCircleScore) 
    {
        playerCircleScore=this.playerCircleScore.Value;
        playerCrossScore=this.playerCrossScore.Value;    
    }
}
