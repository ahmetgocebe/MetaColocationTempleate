using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

/// <summary>
/// Handles spawning objects on the server when the game starts.
/// </summary>
public class HostController : NetworkBehaviour
{
    /// <summary>
    /// List of prefabs to spawn when the server starts.
    /// </summary>
    [Tooltip("List of prefabs that will be spawned when the server starts.")]
    public List<GameObject> SpawnOnStart = new List<GameObject>();

    /// <summary>
    /// Called when the server starts. Spawns objects and sets the initial position of the host.
    /// </summary>
    public override void OnStartServer()
    {
        base.OnStartServer();

        // Only execute if this is the local player and server.
        if (isLocalPlayer && isServer)
        {
            StartCoroutine(Spawn());

            // Set the initial position of the host.
            transform.position = Vector3.one * -5f;
        }
    }

    /// <summary>
    /// Coroutine to spawn objects with a delay between each spawn.
    /// </summary>
    /// <returns>An enumerator for coroutine execution.</returns>
    IEnumerator Spawn()
    {
        foreach (var item in SpawnOnStart)
        {
            // Instantiate the prefab.
            GameObject g = Instantiate(item);

            // Spawn the object on the network and associate it with this game object.
            NetworkServer.Spawn(g, this.gameObject);

            // Wait for a short delay before spawning the next object.
            yield return new WaitForSeconds(0.5f);
        }
    }
}
