using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TicTacToeLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float lobbyUpdateTimer;
    private const float LobbyUpdateTimerMax = 2;
    private const float HeartbeatTimerMax = 15f;

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"Signed in{AuthenticationService.Instance.PlayerId}");
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await CreateLobby();
            await ListLobbies();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPollForUpdate();
    }

    private async Task HandleLobbyHeartbeat()
    {
        try
        {
            if (hostLobby == null) return;

            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                heartbeatTimer = HeartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async Task CreateLobby()
    {
        try
        {
            hostLobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", 2);
            joinedLobby = hostLobby;
            PrintPlayers(hostLobby);
            Debug.Log($"Lobby created: {hostLobby.Name} with id: {hostLobby.Id} max players {hostLobby.MaxPlayers}");
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async Task ListLobbies()
    {
        try
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
            foreach (var lobby in lobbies.Results)
            {
                Debug.Log($"Lobby found: {lobby.Name} with id: {lobby.Id} max players {lobby.MaxPlayers}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async Task JoinLobbyById(String lobbyId)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    private async Task JoinLobbyByCode(String lobbyCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
    
    private async Task QuickJoinLobby()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
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
        try
        {
            if (joinedLobby == null) return;

            lobbyUpdateTimer -= Time.deltaTime;
            if (lobbyUpdateTimer <= 0)
            {
                lobbyUpdateTimer = LobbyUpdateTimerMax;
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private async Task LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }
}