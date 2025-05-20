using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;

    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerChanged;
    
    private PlayerType localPlayerType;
    private PlayerType currentPlayablePlayerType;

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

    public override void OnNetworkSpawn()
    {
        localPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
    }

    private void NetworkManager_OnClientConnectedCallback(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType = PlayerType.Cross;
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
        if (playerType != currentPlayablePlayerType) return;

        Debug.LogFormat("Clicked on grid position {0}, {1}", x, y);
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (currentPlayablePlayerType)
        {
            default:
            case PlayerType.Cross:
                currentPlayablePlayerType = PlayerType.Circle;
                break;
            case PlayerType.Circle:
                currentPlayablePlayerType = PlayerType.Cross;
                break;
        }
        TriggerOnCurrentPlayablePlayerChangedRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnCurrentPlayablePlayerChangedRpc()
    {
        OnCurrentPlayablePlayerChanged?.Invoke(this, EventArgs.Empty);
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
