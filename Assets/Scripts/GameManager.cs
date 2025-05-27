using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int X;
        public int Y;
        public PlayerType PlayerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }

    public struct Line
    {
        public List<Vector2Int> GridVector2IntList;
        public Vector2Int CenterGridPosition;
        public Orientation Orientation;
    }

    private List<Line> lineList;

    public event EventHandler OnGameStarted;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;

    public class OnGameWinEventArgs : EventArgs
    {
        public Line Line;
        public PlayerType WinPlayerType;
    }

    public event EventHandler OnCurrentPlayablePlayerChanged;
    public event EventHandler OnRematch;

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new();
    private PlayerType[,] playerTypeArray = new PlayerType[3, 3];

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

        playerTypeArray = new PlayerType[3, 3];

        lineList = new List<Line>
        {
            // Horizontal lines
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(0, 0), new(1, 0), new(2, 0) },
                CenterGridPosition = new Vector2Int(1, 0),
                Orientation = Orientation.Horizontal
            },
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(0, 1), new(1, 1), new(2, 1) },
                CenterGridPosition = new Vector2Int(1, 1),
                Orientation = Orientation.Horizontal
            },
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(0, 2), new(1, 2), new(2, 2) },
                CenterGridPosition = new Vector2Int(1, 2),
                Orientation = Orientation.Horizontal
            },

            // Vertical lines
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(0, 0), new(0, 1), new(0, 2) },
                CenterGridPosition = new Vector2Int(0, 1),
                Orientation = Orientation.Vertical
            },
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(1, 0), new(1, 1), new(1, 2) },
                CenterGridPosition = new Vector2Int(1, 1),
                Orientation = Orientation.Vertical
            },
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(2, 0), new(2, 1), new(2, 2) },
                CenterGridPosition = new Vector2Int(2, 1),
                Orientation = Orientation.Vertical
            },

            // Diagonal lines
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(0, 0), new(1, 1), new(2, 2) },
                CenterGridPosition = new Vector2Int(1, 1),
                Orientation = Orientation.DiagonalA
            },
            new()
            {
                GridVector2IntList = new List<Vector2Int>
                    { new(0, 2), new(1, 1), new(2, 0) },
                CenterGridPosition = new Vector2Int(1, 1),
                Orientation = Orientation.DiagonalB
            }
        };
    }

    public override void OnNetworkSpawn()
    {
        localPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        currentPlayablePlayerType.OnValueChanged += (_, _) =>
        {
            OnCurrentPlayablePlayerChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value) return;

        if (playerTypeArray[x, y] != PlayerType.None) return;

        playerTypeArray[x, y] = playerType;

        Debug.LogFormat("Clicked on grid position {0}, {1}", x, y);
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            X = x,
            Y = y,
            PlayerType = playerType
        });

        switch (currentPlayablePlayerType.Value)
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

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None &&
               aPlayerType == bPlayerType &&
               aPlayerType == cPlayerType;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            playerTypeArray[line.GridVector2IntList[0].x, line.GridVector2IntList[0].y],
            playerTypeArray[line.GridVector2IntList[1].x, line.GridVector2IntList[1].y],
            playerTypeArray[line.GridVector2IntList[2].x, line.GridVector2IntList[2].y]);
    }

    private void TestWinner()
    {
        for (var i = 0; i < lineList.Count; i++)
        {
            var line = lineList[i];
            if (TestWinnerLine(line))
            {
                Debug.Log("Winner: " + line.CenterGridPosition);
                currentPlayablePlayerType.Value = PlayerType.None;
                TriggerOnGameWinRpc(i, playerTypeArray[line.CenterGridPosition.x, line.CenterGridPosition.y]);
                break;
            }
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            Line = line,
            WinPlayerType = winPlayerType
        });
    }

    public PlayerType GetLocalPlayerType()
    {
        return localPlayerType;
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int x = 0; x < playerTypeArray.GetLength(0); x++)
        {
            for (int y = 0; y < playerTypeArray.GetLength(1); y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }
        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }
}
