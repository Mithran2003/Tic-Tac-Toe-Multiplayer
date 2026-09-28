using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System;

public class NetworkRelay : MonoBehaviour
{
    private string joinCode;
    public static NetworkRelay Instance {get;private set;}
    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("More than one NetworkRelay instance detected!");
        }
        Instance = this;
    }
    private async void Start()
    {
       await UnityServices.InitializeAsync();
       await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task<string> StartHostRelay(int maxConnections=1)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation.RelayServer.IpV4,(ushort)allocation.RelayServer.Port,allocation.AllocationIdBytes,allocation.ConnectionData,allocation.ConnectionData,allocation.Key,true,true));
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return NetworkManager.Singleton.StartHost()?joinCode:null;
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
        
    }

    public  async void StartHost()
    {
        joinCode = await StartHostRelay();
        Debug.Log(joinCode);
    }

    public async void joinRelay(string joinCode)
    {
        await StartClientRelay(joinCode);
    }

    public string GetJoinCode()
    {
        return joinCode;
    }

    private async Task<bool> StartClientRelay(string joinCode)
    {
        try
        {
             JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation.RelayServer.IpV4,(ushort)joinAllocation.RelayServer.Port,joinAllocation.AllocationIdBytes,joinAllocation.ConnectionData,joinAllocation.ConnectionData,joinAllocation.Key,true,true));
            return !string.IsNullOrEmpty(joinCode)&& NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e)
        {
            Debug.Log(e);
            return false;
        }
       
    }
   
}
