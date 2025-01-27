using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;

/// <summary>
/// Manages network connections and handles starting as a host or client,
/// with support for Oculus VR input and discovery.
/// </summary>
public class NetworkController : MonoBehaviour
{
    /// <summary>
    /// Reference to the NetworkManager component to manage network actions.
    /// </summary>
    public NetworkManager networkManager;

    /// <summary>
    /// Reference to the NetworkDiscovery component for server discovery.
    /// </summary>
    public NetworkDiscovery networkDiscovery;

    /// <summary>
    /// Indicates whether the current instance is acting as a host.
    /// </summary>
    public bool isHost = false;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes the Oculus platform if an HMD is present.
    /// </summary>
    private void Awake()
    {
        if (OVRManager.isHmdPresent)
        {
            // Initialize Oculus Platform
            Oculus.Platform.Core.Initialize();
        }
    }

    /// <summary>
    /// Called before the first frame update.
    /// Attempts to start the network as a client, or as a host if no server is found.
    /// </summary>
    void Start()
    {
        TryStartNetwork();
    }

    /// <summary>
    /// Tries to start the network by discovering servers.
    /// If the "Y" button on the Oculus controller is pressed, starts as a host.
    /// </summary>
    void TryStartNetwork()
    {
        networkDiscovery.StartDiscovery();

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            // Try to connect as a client
            Debug.Log("Attempting to connect as a client...");
            networkManager.StartHost();
            networkDiscovery.AdvertiseServer();
        }
    }

    /// <summary>
    /// Called when a server is found during the discovery process.
    /// Connects the client to the discovered server using its URI.
    /// </summary>
    /// <param name="response">The response containing server details.</param>
    public void OnServerFound(ServerResponse response)
    {
        networkManager.StartClient(response.uri);
    }
}
