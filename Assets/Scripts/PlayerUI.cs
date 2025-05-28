using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject arrowCross;
    [SerializeField] private GameObject arrowCircle;
    [SerializeField] private GameObject youTextCross;
    [SerializeField] private GameObject youTextCircle;
    [SerializeField] private TextMeshProUGUI playerCrossScoreText;
    [SerializeField] private TextMeshProUGUI playerCircleScoreText;


    private void Awake()
    {
        arrowCross.SetActive(false);
        arrowCircle.SetActive(false);
        youTextCross.SetActive(false);
        youTextCircle.SetActive(false);
        
        playerCrossScoreText.text = "";
        playerCircleScoreText.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerChanged += GameManager_OnCurrentPlayablePlayerChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged(object sender, EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);
        
        playerCrossScoreText.text = playerCrossScore.ToString();
        playerCircleScoreText.text = playerCircleScore.ToString();
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
        
        playerCrossScoreText.text = "0";
        playerCircleScoreText.text = "0";

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
