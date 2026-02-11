using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.UI;

/// <summary>
/// UNIVERSAL LOBBY MANAGER
/// Works with universal player controller
/// Path: Assets/Scripts/Multiplayer/LobbyManager.cs
/// 
/// SETUP:
/// 1. One player prefab for everyone
/// 2. Automatic positioning (host left, client right)
/// 3. No need for separate enemy/player prefabs
/// </summary>
public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [Header("Settings")]
    public GameObject playerPrefab; // ONE prefab for all players
    public GameObject uiPanel;
    public Button startGameButton;

    [Header("UI List Settings")]
    public Transform playerListContainer;
    public GameObject playerEntryPrefab;

    private NetworkList<FixedString64Bytes> playerNames;
    private Dictionary<ulong, string> clientNamesMap = new Dictionary<ulong, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        playerNames = new NetworkList<FixedString64Bytes>();
    }

    public override void OnNetworkSpawn()
    {
        playerNames.OnListChanged += (e) => UpdateStatusUI();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
        }

        UpdateStatusUI();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleDisconnect;
        }

        base.OnNetworkDespawn();
    }

    private void HandleDisconnect(ulong clientId)
    {
        if (clientNamesMap.ContainsKey(clientId))
        {
            playerNames.Remove(clientNamesMap[clientId]);
            clientNamesMap.Remove(clientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerNameServerRpc(string name, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!clientNamesMap.ContainsKey(clientId))
        {
            clientNamesMap.Add(clientId, name);
            playerNames.Add(name);
            Debug.Log($"✅ Player '{name}' joined (ID: {clientId})");
        }
    }

    private void UpdateStatusUI()
    {
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            string pName = playerNames[i].ToString();

            var nameText = entry.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = pName;

            Button kickBtn = entry.GetComponentInChildren<Button>();
            if (kickBtn != null)
            {
                if (IsServer)
                {
                    ulong targetId = GetIdFromName(pName);

                    if (targetId == NetworkManager.Singleton.LocalClientId)
                    {
                        kickBtn.gameObject.SetActive(false);
                    }
                    else
                    {
                        kickBtn.onClick.AddListener(() => KickPlayer(targetId));
                    }
                }
                else
                {
                    kickBtn.gameObject.SetActive(false);
                }
            }
        }

        if (IsServer && startGameButton != null)
        {
            startGameButton.interactable = playerNames.Count >= 2;
        }
    }

    private ulong GetIdFromName(string name)
    {
        foreach (var pair in clientNamesMap)
        {
            if (pair.Value == name)
                return pair.Key;
        }
        return 999;
    }

    public void KickPlayer(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"🚫 Kicking client {clientId}");

        NotifyKickedClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        });

        StartCoroutine(DisconnectDelay(clientId));
    }

    [ClientRpc]
    private void NotifyKickedClientRpc(ClientRpcParams rpcParams = default)
    {
        if (LocalNetworkUI.Instance != null)
        {
            LocalNetworkUI.Instance.ShowKickMessage("You have been kicked!");
        }
    }

    private System.Collections.IEnumerator DisconnectDelay(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    //public void StartGame()
    //{
    //    if (!IsServer) return;

    //    Debug.Log("🎮 Starting game...");

    //    uiPanel.SetActive(false);
    //    HideUIClientRpc();

    //    // Get list of connected clients
    //    var connectedClients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);

    //    for (int i = 0; i < connectedClients.Count; i++)
    //    {
    //        ulong clientId = connectedClients[i];
    //        GameObject playerInstance = Instantiate(playerPrefab);

    //        // Set position BEFORE spawning
    //        if (i == 0) // First player (Host) - LEFT side
    //        {
    //            playerInstance.transform.position = new Vector3(-5f, 0f, 0f);
    //            Debug.Log($"👈 HOST spawning at LEFT (-5, 0)");
    //        }
    //        else // Second player (Client) - RIGHT side
    //        {
    //            playerInstance.transform.position = new Vector3(5f, 0f, 0f);
    //            Debug.Log($"👉 CLIENT spawning at RIGHT (5, 0)");
    //        }

    //        // Spawn as network object
    //        var netObj = playerInstance.GetComponent<NetworkObject>();
    //        netObj.SpawnAsPlayerObject(clientId);

    //        // Sync sprite facing on ALL clients
    //        if (i == 0)
    //        {
    //            SetPlayerFacingClientRpc(netObj.NetworkObjectId, true); // Host faces RIGHT
    //        }
    //        else
    //        {
    //            SetPlayerFacingClientRpc(netObj.NetworkObjectId, false); // Client faces LEFT
    //        }

    //        Debug.Log($"✅ Player {i} (ClientId: {clientId}) spawned successfully");
    //    }
    //}

    public void StartGame()
    {
        if (!IsServer) return;

        Debug.Log("🎮 Starting game...");

        uiPanel.SetActive(false);
        HideUIClientRpc();

        // Get list of connected clients
        var connectedClients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);

        for (int i = 0; i < connectedClients.Count; i++)
        {
            ulong clientId = connectedClients[i];
            GameObject playerInstance = Instantiate(playerPrefab);

            // Set position BEFORE spawning
            if (i == 0) // First player (Host) - LEFT side
            {
                playerInstance.transform.position = new Vector3(-5f, -2f, 0f); // Adjust Y as needed
                playerInstance.transform.rotation = Quaternion.identity;
                Debug.Log($"👈 HOST gun spawning at LEFT (-5, -2)");
            }
            else // Second player (Client) - RIGHT side
            {
                playerInstance.transform.position = new Vector3(5f, -2f, 0f); // Adjust Y as needed
                playerInstance.transform.rotation = Quaternion.identity;
                Debug.Log($"👉 CLIENT gun spawning at RIGHT (5, -2)");
            }

            // Spawn as network object
            var netObj = playerInstance.GetComponent<NetworkObject>();
            netObj.SpawnAsPlayerObject(clientId);

            // Sync sprite facing on ALL clients
            if (i == 0)
            {
                SetPlayerFacingClientRpc(netObj.NetworkObjectId, true); // Host faces RIGHT
            }
            else
            {
                SetPlayerFacingClientRpc(netObj.NetworkObjectId, false); // Client faces LEFT
            }

            Debug.Log($"✅ Player {i} (ClientId: {clientId}) spawned successfully");
        }
    }

    [ClientRpc]
    private void SetPlayerFacingClientRpc(ulong networkObjectId, bool faceRight)
    {
        // Find the player by NetworkObjectId
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            Vector3 scale = netObj.transform.localScale;
            scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            netObj.transform.localScale = scale;

            Debug.Log($"🎨 Set sprite facing: {(faceRight ? "RIGHT →" : "LEFT ←")} for NetworkObject {networkObjectId}");
        }
    }

    [ClientRpc]
    private void HideUIClientRpc()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }
}