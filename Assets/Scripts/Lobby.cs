using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in" + AuthenticationService.Instance.PlayerId);
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

    private async Task CreateLobby()
    {
        try
        {
            var lobby = await LobbyService.Instance.CreateLobbyAsync("Lobby", 2);
            Debug.Log("Lobby created: " + lobby.Name + " with id: " + lobby.Id + " max players " + lobby.MaxPlayers);
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
            var lobbies = await LobbyService.Instance.QueryLobbiesAsync();
            foreach (var lobby in lobbies.Results)
            {
                Debug.Log("Lobby found: " + lobby.Name + " with id: " + lobby.Id + " max players " + lobby.MaxPlayers);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
    }
}
