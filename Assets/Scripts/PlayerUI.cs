using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject arrowCross;
    [SerializeField] private GameObject arrowCircle;
    [SerializeField] private GameObject youTextCross;
    [SerializeField] private GameObject youTextCircle;


    private void Awake()
    {
        arrowCross.SetActive(false);
        arrowCircle.SetActive(false);
        youTextCross.SetActive(false);
        youTextCircle.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerChanged += GameManager_OnCurrentPlayablePlayerChanged;
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            youTextCross.SetActive(true);
        }
        else
        {
            youTextCircle.SetActive(true);
        }

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            arrowCross.SetActive(true);
            arrowCircle.SetActive(false);
        }
        else
        {
            arrowCross.SetActive(false);
            arrowCircle.SetActive(true);
        }
    }

    private void GameManager_OnCurrentPlayablePlayerChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }
}
