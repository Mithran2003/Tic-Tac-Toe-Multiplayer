using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXprefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject+=GameManager_OnPlacedObject;
    }

    private void GameManager_OnPlacedObject(object sender,EventArgs e) 
    {
        Transform sfxTransform=Instantiate(placeSFXprefab);    
        Destroy(sfxTransform.gameObject,5f);
    }
}
