using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Oculus.Platform;
using Oculus.Platform.Models;
using System;

/// <summary>
/// Manages player-related network behavior, including Oculus VR integration and player-specific logic.
/// </summary>
public class PlayerNetworkController : NetworkBehaviour
{
    /// <summary>
    /// Reference to the OVRManager component for handling Oculus-specific functionality.
    /// </summary>
    public OVRManager oVRManager;

    /// <summary>
    /// Reference to the player's head object (e.g., for VR tracking).
    /// </summary>
    public GameObject head;

    /// <summary>
    /// Reference to the player's "monke" object (custom game object representation).
    /// </summary>
    public GameObject monke;

    /// <summary>
    /// The unique ID of the player, synchronized across the network.
    /// </summary>
    [SyncVar] public uint idOfPlayer;

    /// <summary>
    /// Called before the first frame update. Initializes player-specific settings for the local player.
    /// </summary>
    void Start()
    {
        if (isLocalPlayer)
        {
            RemoveHeadFromCameraRenderableLayer();
            CMDOnSetPlayerId(netId);
        }
    }

    /// <summary>
    /// Called once per frame. Updates the "monke" object's active state based on the player ID.
    /// </summary>
    private void Update()
    {
        monke.SetActive(idOfPlayer != 1);
    }

    /// <summary>
    /// Command method called on the server to set the player ID.
    /// </summary>
    /// <param name="id">The network ID of the player.</param>
    [Command]
    private void CMDOnSetPlayerId(uint id)
    {
        idOfPlayer = netId;
    }

    /// <summary>
    /// Removes the player's head object from the camera's renderable layer to avoid rendering issues.
    /// </summary>
    private void RemoveHeadFromCameraRenderableLayer()
    {
        int layerIndex = LayerMask.NameToLayer("Monke");
        head = GameObject.Find("CenterEyeAnchor");
        monke.layer = LayerMask.NameToLayer("Monke");
        head.GetComponent<Camera>().cullingMask &= ~(1 << layerIndex);
    }

    /// <summary>
    /// Called when the Host stops. Handles cleanup logic if the player is the server and local player.
    /// </summary>
    public override void OnStopClient()
    {
        base.OnStopClient();
        if (isServer && isLocalPlayer)
        {
            DropAll();
        }
    }

    /// <summary>
    /// Executes logic to drop all resources or connections when the player disconnects.
    /// </summary>
    public void DropAll()
    {
        Debug.Log("Dropping all");
        RpcDropAll();
    }

    /// <summary>
    /// Client RPC method to broadcast a message to all clients and quit the application.
    /// </summary>
    [ClientRpc]
    private void RpcDropAll()
    {
        Debug.Log("All dropped");

        UnityEngine.Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }
}
