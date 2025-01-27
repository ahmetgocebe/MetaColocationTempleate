using Mirror;
using Mirror.Discovery;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CustomNetworkManager : NetworkManager
{
    public NetworkDiscovery networkDiscovery;
    public UnityEvent OnServerStopped;

    private void Start()
    {
        if (IsDesktopPlatform())
        {
            // Start as a host if on desktop platform
            StartHost();
            Debug.Log("Started Host on Desktop");
        }
        else
        {
            // Start discovering hosts on mobile or other platforms
            StartCoroutine(TryDiscoverHosts());
        }
    }
 
    // Method to detect if the platform is desktop
    private bool IsDesktopPlatform()
    {
        return Application.platform == RuntimePlatform.WindowsPlayer ||
               Application.platform == RuntimePlatform.OSXPlayer ||
               Application.platform == RuntimePlatform.LinuxPlayer ||
               Application.isEditor;  // Consider the editor as desktop for testing
    }
    public override void OnStopServer()
    {
        base.OnStopServer();
        OnServerStopped?.Invoke();
    }
    // Coroutine to discover hosts every 3 seconds on non-desktop platforms
    private IEnumerator TryDiscoverHosts()
    {
        networkDiscovery.StartDiscovery();

        while (!NetworkClient.isConnected)
        {
            yield return new WaitForSeconds(3f);

            if (NetworkClient.isConnected)
            {
                Debug.Log("Connected to Host");
                break;
            }

            Debug.Log("Trying to discover hosts...");
        }
    }

    // Called when a server is found through network discovery
    public void OnDiscoveredServer(ServerResponse info)
    {
        Debug.Log("Discovered host: " + info.uri);
        // Stop discovering once a host is found
        networkDiscovery.StopDiscovery();
        // Join the discovered host
        NetworkManager.singleton.StartClient(info.uri);
    }
}
