using TMPro;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private TMP_InputField JoinCodeField;
    [SerializeField] private TextMeshProUGUI JoinCodeDisplayText;
    [SerializeField] private Image Background;

    private void Awake()
    {
        StartHostButton.onClick.AddListener(()=>{NetworkRelay.Instance.StartHost();
        Hide();
        });
        StartClientButton.onClick.AddListener(()=>{NetworkRelay.Instance.joinRelay(JoinCodeField.text);
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
