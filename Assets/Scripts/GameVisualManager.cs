using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GridSize = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;

    private List<GameObject> visualGameObjectList;

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnRematch += GameManager_OnRematch;
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.X, e.Y, e.PlayerType);
    }

    private void GameManager_OnRematch(object sender, EventArgs e)
    {
        visualGameObjectList.ForEach(Destroy);
        visualGameObjectList.Clear();
    }
    
    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        float eulerZ;
        switch (e.Line.Orientation)
        {
            default:
            case GameManager.Orientation.Horizontal: eulerZ = 0; break;
            case GameManager.Orientation.Vertical: eulerZ = 90; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45; break;
        }
        Transform lineCompleteTransform =
            Instantiate(
                lineCompletePrefab,
                GetGridWorldPosition(e.Line.CenterGridPosition.x, e.Line.CenterGridPosition.y),
                Quaternion.Euler(0, 0, eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(lineCompleteTransform.gameObject);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform prefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                prefab = crossPrefab;
                break;
            case GameManager.PlayerType.Circle:
                prefab = circlePrefab;
                break;
        }
        var spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
        visualGameObjectList.Add(spawnedCrossTransform.gameObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GridSize + x * GridSize, -GridSize + y * GridSize);
    }
}
