using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXprefab;
    [SerializeField] private Transform WinSFXprefab;
    [SerializeField] private Transform LoseSFXprefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject+=GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin+=GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender,GameManager.OnGameWinEventArgs e) 
    {
        if(GameManager.Instance.GetLocalPlayerType()==e.winPlayerType)
        {
            Transform sfxTransform=Instantiate(WinSFXprefab);    
            Destroy(sfxTransform.gameObject,5f);
        } 
        else
        {
            Transform sfxTransform=Instantiate(LoseSFXprefab);    
            Destroy(sfxTransform.gameObject,5f);
        }   
    }

    private void GameManager_OnPlacedObject(object sender,EventArgs e) 
    {
        Transform sfxTransform=Instantiate(placeSFXprefab);    
        Destroy(sfxTransform.gameObject,5f);
    }
}
