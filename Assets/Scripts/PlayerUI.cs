using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
   [SerializeField] private GameObject CrossArrowGameObject;
   [SerializeField] private GameObject CircleArrowGameObject;
   [SerializeField] private GameObject CrossYouTMPGameObject;
   [SerializeField] private GameObject CircleYouTMPGameObject;

    private void Awake()
    {
        CrossArrowGameObject.SetActive(false);
        CircleArrowGameObject.SetActive(false);
        CircleYouTMPGameObject.SetActive(false);
        CrossYouTMPGameObject.SetActive(false);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted+=GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChange+=GameManager_OnCurrentPlayablePlayerTypeChange;
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
}
