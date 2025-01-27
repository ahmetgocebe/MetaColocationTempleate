using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using Oculus.Platform;
using UnityEngine;

/// <summary>
/// Manages the placement, creation, sharing, and loading of spatial anchors using OVRSpatialAnchor for Oculus spatial anchors.
/// </summary>
public class AnchorManager : NetworkBehaviour
{
    /// <summary>
    /// Reference to the right-hand GameObject for VR interactions.
    /// </summary>
    [HideInInspector] public GameObject rightHand;

    /// <summary>
    /// Prefab used to create spatial anchors in the scene.
    /// </summary>
    [Tooltip("Prefab used to create spatial anchors.")]
    public GameObject anchorPrefab;

    /// <summary>
    /// Reference to the OVRSpatialAnchor component associated with the anchor.
    /// </summary>
    [HideInInspector] public OVRSpatialAnchor anchor;

    /// <summary>
    /// Placeholder object representing where the anchor will be placed.
    /// </summary>
    [Tooltip("Placeholder object to visualize the anchor placement.")]
    public GameObject anchorPlaceholder;

    /// <summary>
    /// The App ID for initializing the Oculus platform.
    /// </summary>
    [Tooltip("App ID for Oculus Platform SDK initialization.")]
    public string appId;

    /// <summary>
    /// The user ID for sharing anchors with other users.
    /// </summary>
    [Tooltip("User ID for sharing anchors.")]
    public ulong userId;

    /// <summary>
    /// Movement speed for anchor placeholder adjustments.
    /// </summary>
    [Tooltip("Speed at which the anchor placeholder moves.")]
    public float moveSpeed = 1f;

    /// <summary>
    /// Rotation speed for adjusting the anchor placeholder orientation.
    /// </summary>
    [Tooltip("Speed at which the anchor placeholder rotates.")]
    public float rotationSpeed = 50f;

    private bool isPlaced = false;
    private List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();

    /// <summary>
    /// Initializes the Oculus Platform SDK with the provided app ID.
    /// </summary>
    private void Awake()
    {
        Core.Initialize(appId);
    }

    /// <summary>
    /// Called when the local player starts. Sets up right-hand tracking, placeholder, and loads the anchor if necessary.
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        if (!isLocalPlayer) return;

        rightHand = GameObject.Find("RightControllerAnchor");
        anchorPlaceholder = GameObject.Find("AnchorPlaceholder");
        anchorPlaceholder.transform.position = Vector3.zero;
        isPlaced = isServer;

        if (isServer)
        {
            Instantiate(anchorPrefab);
            anchorPlaceholder.gameObject.SetActive(false);
            return;
        }

        StartCoroutine(WaitForNetworkToLoadAnchor());
    }

    /// <summary>
    /// Coroutine to wait for the network to initialize and load anchors by UUID if available.
    /// </summary>
    IEnumerator WaitForNetworkToLoadAnchor()
    {
        yield return new WaitUntil(() => Core.IsInitialized());
        yield return new WaitUntil(() => AnchorFileSystem.Instance != null);

        if (!string.IsNullOrEmpty(AnchorFileSystem.Instance.Anchor))
        {
            LoadAnchorsByUuid(new List<Guid>() { Guid.Parse(AnchorFileSystem.Instance.Anchor) });
        }
    }

    /// <summary>
    /// Updates the anchor placement logic, including VR input handling and placeholder adjustments.
    /// </summary>
    private void Update()
    {
        if (!Core.IsInitialized())
        {
            Core.Initialize(appId);
            return;
        }

        if (isLocalPlayer)
        {
            anchorPlaceholder.SetActive(!isPlaced);
        }

        if (isServer || !isLocalPlayer || isPlaced) return;

        MovePlaceHolder();

        if (rightHand == null)
        {
            rightHand = GameObject.Find("RightControllerAnchor");
        }

        if (!isPlaced && OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("Begin the anchor");
            isPlaced = true;
            PlaceAnchor();
        }
    }

    /// <summary>
    /// Handles movement and rotation of the anchor placeholder based on VR controller input.
    /// </summary>
    private void MovePlaceHolder()
    {
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            anchorPlaceholder.transform.position = rightHand.transform.position;
            anchorPlaceholder.transform.rotation = rightHand.transform.rotation;
        }

        Vector2 rightThumbstick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
        Vector3 moveDirection = new Vector3(rightThumbstick.x, 0, rightThumbstick.y);
        anchorPlaceholder.transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        Vector2 leftThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        float rotationAmount = leftThumbstick.x * rotationSpeed * Time.deltaTime;
        anchorPlaceholder.transform.Rotate(Vector3.up, rotationAmount, Space.World);
    }

    /// <summary>
    /// Initiates the process to place a spatial anchor.
    /// </summary>
    private void PlaceAnchor()
    {
        StartCoroutine(CreateSpatialAnchor());
    }

    /// <summary>
    /// Coroutine to create a spatial anchor, match its position to the placeholder, and save it asynchronously.
    /// </summary>
    IEnumerator CreateSpatialAnchor()
    {
        var go = Instantiate(anchorPrefab);
        go.transform.position = anchorPlaceholder.transform.position;
        go.transform.eulerAngles = anchorPlaceholder.transform.eulerAngles;

        var anchor = go.AddComponent<OVRSpatialAnchor>();

        yield return new WaitUntil(() => anchor.Created);
        Debug.Log("Anchor created");

        Debug.Log($"Created anchor {anchor.Uuid}");
        SaveAnchor(anchor);
    }

    /// <summary>
    /// Saves the anchor asynchronously and stores its UUID in player preferences.
    /// </summary>
    /// <param name="anchor">The OVRSpatialAnchor to save.</param>
    public async void SaveAnchor(OVRSpatialAnchor anchor)
    {
        var result = await anchor.SaveAnchorAsync();
        if (result.Success)
        {
            PlayerPrefs.SetString("Anchor", anchor.Uuid.ToString());
            Debug.Log($"Anchor {anchor.Uuid} saved successfully.");
            ShareAnchor(anchor);
        }
        else
        {
            Debug.LogError($"Failed to save anchor {anchor.Uuid} with error {result.Status}");
        }
    }

    /// <summary>
    /// Shares the anchor with another user asynchronously.
    /// </summary>
    /// <param name="anchor">The OVRSpatialAnchor to share.</param>
    private async void ShareAnchor(OVRSpatialAnchor anchor)
    {
        if (OVRSpaceUser.TryCreate(userId, out OVRSpaceUser myUser))
        {
            var result = await anchor.ShareAsync(myUser);
            if (result == OVRSpatialAnchor.OperationResult.Success)
            {
                Debug.Log("Anchor shared successfully");
                CmdSaveAnchor(anchor.Uuid.ToString());
            }
            else
            {
                Debug.LogError("Failed to share the anchor");
            }
        }
        else
        {
            Debug.LogError("Failed to create OVRSpaceUser");
        }
    }

    /// <summary>
    /// Command to save the anchor on the server.
    /// </summary>
    /// <param name="anchorId">The UUID of the anchor to save.</param>
    [Command]
    private void CmdSaveAnchor(string anchorId)
    {
        Debug.Log("CmdSaveAnchor");
        AnchorFileSystem.Instance.SaveAnchorToXml(anchorId);
        RpcSaveAnchor(anchorId);
    }

    /// <summary>
    /// RPC to save the anchor on the client and attempt to load it by UUID.
    /// </summary>
    /// <param name="anchorId">The UUID of the anchor to save.</param>
    [ClientRpc]
    private void RpcSaveAnchor(string anchorId)
    {
        if (isServer || isPlaced) return;

        Debug.Log("RpcSaveAnchor on client");
        Guid g = new Guid(PlayerPrefs.GetString("Anchor"));
        Debug.Log("Parsed GUID " + g);

        if (g != Guid.Empty)
        {
            LoadAnchorsByUuid(new List<Guid>() { g });
        }
    }

    /// <summary>
    /// Loads anchors by their UUID and localizes them for use in the scene.
    /// </summary>
    /// <param name="uuids">List of UUIDs to load and localize.</param>
    async void LoadAnchorsByUuid(IEnumerable<Guid> uuids)
    {
        foreach (var uuid in uuids)
        {
            Debug.Log($"Attempting to load anchor with UUID: {uuid}");
        }

        var result = await OVRSpatialAnchor.LoadUnboundSharedAnchorsAsync(uuids, _unboundAnchors);
        Debug.Log("Status of loading unbound anchors: " + result.Status);

        if (result.Success)
        {
            Debug.Log($"Anchors loaded successfully: {_unboundAnchors.Count} unbound anchors");

            foreach (var unboundAnchor in result.Value)
            {
                Debug.Log($"Unbound anchor UUID: {unboundAnchor.Uuid}");
                unboundAnchor.LocalizeAsync().ContinueWith((success, anchor) =>
                {
                    if (success)
                    {
                        var spatialAnchor = Instantiate(anchorPrefab).AddComponent<OVRSpatialAnchor>();
                        Debug.Log($"Anchor localized successfully with UUID: {unboundAnchor.Uuid}");
                        unboundAnchor.BindTo(spatialAnchor);
                    }
                    else
                    {
                        Debug.LogError($"Localization failed for anchor {unboundAnchor.Uuid}");
                    }
                }, unboundAnchor);
            }
            isPlaced = true;
        }
        else
        {
            await Task.Delay(1000);
            LoadAnchorsByUuid(uuids);
            Debug.LogError($"Load failed with error: {result.Status}. No anchors loaded.");
        }
    }
}
