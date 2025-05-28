using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private const float AudioCleanupDelay = 5f;

    [SerializeField] private Transform placeSfxPrefab;
    [SerializeField] private Transform winSfxPrefab;
    [SerializeField] private Transform loseSfxPrefab;

    private void Start()
    {
        GameManager.Instance.OnObjectPlaced += GameManager_OnObjectPlaced;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void OnDisable()
    {
            GameManager.Instance.OnObjectPlaced -= GameManager_OnObjectPlaced;
            GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        var sfxPrefab = e.WinPlayerType == GameManager.Instance.GetLocalPlayerType() 
            ? winSfxPrefab 
            : loseSfxPrefab;
            
        PlaySound(sfxPrefab);
    }

    private void GameManager_OnObjectPlaced(object sender, EventArgs e)
    {
        PlaySound(placeSfxPrefab);
    }

    private void PlaySound(Transform sfxPrefab)
    {
        var sfxInstance = Instantiate(sfxPrefab);
        Destroy(sfxInstance.gameObject, AudioCleanupDelay);
    }
}