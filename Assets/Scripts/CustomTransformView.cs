using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/// <summary>
/// Synchronizes the transform of an object relative to a shared reference point (FlagPole) across the network.
/// </summary>
public class CustomTransformView : NetworkBehaviour
{
    /// <summary>
    /// Reference to the FlagPole instance, used as a shared reference point for synchronization.
    /// </summary>
    [Tooltip("Reference to the FlagPole instance, used as a shared reference point for synchronization.")]
    public FlagPole flagPole;

    /// <summary>
    /// Reference to the PlayerNetworkController associated with this object.
    /// </summary>
    [Tooltip("Reference to the PlayerNetworkController associated with this object.")]
    [SerializeField] private PlayerNetworkController playerNetworkController;

    private Vector3 latestRelativePos;
    private Quaternion latestRelativeRot;
    private float lerpRate = 5f;
    private float nextSendTime;

    private Vector3 receivedRelativePos;
    private Quaternion receivedRelativeRot;

    /// <summary>
    /// Updates the transform of the object, synchronizing it relative to the FlagPole.
    /// </summary>
    void Update()
    {
        // Ensure the FlagPole instance is assigned.
        if (flagPole == null)
        {
            flagPole = FlagPole.Instance;
            return;
        }

        // If this is the local player, calculate and send the relative transform.
        if (isLocalPlayer)
        {
            Vector3 currentPosition = transform.position;
            Quaternion currentRotation = transform.rotation;

            // Calculate the relative position and rotation.
            latestRelativePos = flagPole.transform.InverseTransformPoint(transform.position);
            latestRelativeRot = Quaternion.Inverse(flagPole.transform.rotation) * transform.rotation;

            // Send the relative transform to the server.
            CmdSendRelativeTransform(latestRelativePos, latestRelativeRot);
        }
        else
        {
            // Interpolate position and rotation for non-local players.
            transform.position = Vector3.Lerp(transform.position, flagPole.transform.TransformPoint(receivedRelativePos), lerpRate);
            transform.rotation = flagPole.transform.rotation * receivedRelativeRot;
        }
    }

    /// <summary>
    /// Sends the relative position and rotation from the client to the server.
    /// </summary>
    /// <param name="relativePos">The position of the object relative to the FlagPole.</param>
    /// <param name="relativeRot">The rotation of the object relative to the FlagPole.</param>
    [Command]
    private void CmdSendRelativeTransform(Vector3 relativePos, Quaternion relativeRot)
    {
        // Update the relative transform on all clients.
        RpcUpdateRelativeTransform(relativePos, relativeRot);
    }

    /// <summary>
    /// Updates the relative position and rotation on clients.
    /// </summary>
    /// <param name="relativePos">The position of the object relative to the FlagPole.</param>
    /// <param name="relativeRot">The rotation of the object relative to the FlagPole.</param>
    [ClientRpc]
    private void RpcUpdateRelativeTransform(Vector3 relativePos, Quaternion relativeRot)
    {
        if (!isLocalPlayer)
        {
            // Store the received relative transform for interpolation.
            receivedRelativePos = relativePos;
            receivedRelativeRot = relativeRot;
        }
    }
}
