using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class TicTacToeLobby : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button refreshLobbyButton;
    [SerializeField] private VerticalLayoutGroup lobbyList;
    
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private const float LobbyUpdateTimerMax = 2;
    private const float HeartbeatTimerMax = 15f;

    private async void Start()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in{AuthenticationService.Instance.PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }, "Initialize Unity Services failed" );

        createLobbyButton.onClick.AddListener(async () => await CreateLobby());
        refreshLobbyButton.onClick.AddListener(async () => await ListLobbies());
    }

    private async void Update()
    {
        await HandleLobbyHeartbeat();
        await HandleLobbyPollForUpdate();
    }

    private async Task HandleLobbyHeartbeat()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            if (hostLobby == null) return;

            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                heartbeatTimer = HeartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }, "Heartbeat ping failed" );
    }

    private async Task CreateLobby()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            hostLobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", 2);
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
            Debug.Log($"Lobby created: {hostLobby.Name} with id: {hostLobby.Id} max players {hostLobby.MaxPlayers}");
        }, "Lobby creation failed");
    }

    private async Task ListLobbies()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            var options = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new(QueryFilter.FieldOptions.AvailableSlots, value: "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new(asc: false, QueryOrder.FieldOptions.Created)
                }
            };
            var lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);
            foreach (var textMeshProUGUI in lobbyList.gameObject.GetComponentsInChildren<TextMeshProUGUI>())
            {
                Destroy(textMeshProUGUI.gameObject);
            }
            foreach (var lobby in lobbies.Results)
            {
                Debug.Log($"Lobby found: {lobby.Name} with id: {lobby.Id} max players {lobby.MaxPlayers}");
                var go = new GameObject();
                var rtr = go.AddComponent<RectTransform>();
                rtr.pivot = new Vector2(0, 1);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text = lobby.Name;
                go.transform.SetParent(lobbyList.transform);;
            }
        }, "Lobby listing failed");
    }

    private async Task JoinLobbyById(String lobbyId)
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        }, "Join lobby by ID failed");
    }

    private async Task JoinLobbyByCode(String lobbyCode)
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
        }, "Join lobby by code failed");
    }

    private async Task QuickJoinLobby()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
        }, "Quick lobby join failed");
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in lobby {lobby.Name}:");

        foreach (var player in lobby.Players)
        {
            Debug.Log(player.Id);
        }
    }

    private async Task HandleLobbyPollForUpdate()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            if (joinedLobby == null) return;

            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0)
            {
                lobbyUpdateTimer = LobbyUpdateTimerMax;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            }
        }, "Lobby poll for update failed");
    }

    private async Task LeaveLobby()
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }, "Leave lobby failed");
    }
    
    private async Task KickFromLobby(String playerId)
    {
        await ErrorHandler.SafeExecuteAsync(async () =>
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
        }, "Leave kick failed");
    }
}