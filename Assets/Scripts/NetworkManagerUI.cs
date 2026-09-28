using TMPro;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Services.Relay;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private TMP_InputField JoinCodeField;
    [SerializeField] private TextMeshProUGUI JoinCodeDisplayText;
    [SerializeField] private Image Background;

    private void Awake()
    {
        StartHostButton.onClick.AddListener(()=>{try {NetworkRelay.Instance.StartHost();}
        catch(RelayServiceException e)
            {
                Debug.Log(e);
            }
        Hide();
        });
        StartClientButton.onClick.AddListener(()=>{try{if(JoinCodeField.text!= null)NetworkRelay.Instance.joinRelay(JoinCodeField.text);}
        catch
            {
                Debug.Log("Something went Wrong!!!");
            }
        Hide();
        UpdateJoinCode();
        });

    }

    private void FixedUpdate()
    {
       UpdateJoinCode();
    }

    private void Hide() 
    {
        Background.gameObject.SetActive(false);
        StartHostButton.gameObject.SetActive(false);
        StartClientButton.gameObject.SetActive(false);
        JoinCodeField.gameObject.SetActive(false);
    }

    private void UpdateJoinCode() 
    {
        string JoinCode = NetworkRelay.Instance.GetJoinCode();
        JoinCodeDisplayText.text=JoinCode;
    }
}
