using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSfxPrefab;

    private void Start()
    {
        GameManager.Instance.OnObjectPlaced += GameManager_OnObjectPlaced;
    }

    private void GameManager_OnObjectPlaced(object sender, EventArgs e)
    {
        var placeSfx = Instantiate(placeSfxPrefab);
        Destroy(placeSfx.gameObject, 5f);
    }
}
