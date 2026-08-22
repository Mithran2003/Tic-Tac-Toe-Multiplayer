using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
   [SerializeField] private GameObject CrossArrowGameObject;
   [SerializeField] private GameObject CircleArrowGameObject;
   [SerializeField] private GameObject CrossYouTMPGameObject;
   [SerializeField] private GameObject CircleYouTMPGameObject;
   [SerializeField] private TextMeshProUGUI playerCrossScoreTMP;
   [SerializeField] private TextMeshProUGUI playerCircleScoreTMP;

    private void Awake()
    {
        CrossArrowGameObject.SetActive(false);
        CircleArrowGameObject.SetActive(false);
        CircleYouTMPGameObject.SetActive(false);
        CrossYouTMPGameObject.SetActive(false);
        playerCircleScoreTMP.text="";
        playerCrossScoreTMP.text="";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted+=GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChange+=GameManager_OnCurrentPlayablePlayerTypeChange;
        GameManager.Instance.OnScoreChanged+=GameManager_OnScoreChanged;
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChange(object sender,EventArgs e) 
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender,EventArgs e) 
    {
        if(GameManager.Instance.GetLocalPlayerType()==GameManager.PlayerType.Cross)
        {
            CrossYouTMPGameObject.SetActive(true);
        }
        else
        {
            CircleYouTMPGameObject.SetActive(true);
        }
        playerCircleScoreTMP.text="0";
        playerCrossScoreTMP.text="0";
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow() 
    {
        if(GameManager.Instance.GetCurrentPlayablePlayerType()==GameManager.PlayerType.Cross)
        {
            CrossArrowGameObject.SetActive(true);
            CircleArrowGameObject.SetActive(false);
        }
        else
        {
            CrossArrowGameObject.SetActive(false);
            CircleArrowGameObject.SetActive(true);
        }    
    }

    private void GameManager_OnScoreChanged(object sender,EventArgs e) 
    { 
        GameManager.Instance.GetScore(out int playerCrossScore,out int playerCircleScore);
        playerCrossScoreTMP.text=playerCrossScore.ToString();
        playerCircleScoreTMP.text=playerCircleScore.ToString();
    }
}
