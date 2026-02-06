using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.UI;

public class LobbyManager : NetworkBehaviour
{
    public static LobbyManager Instance;

    [Header("Settings")]
    public GameObject playerPrefab;
    public GameObject uiPanel;
    public Button startGameButton;

    [Header("UI List Settings")]
    public Transform playerListContainer; // 'PlayerList_Image' with Vertical Layout Group 
    public GameObject playerEntryPrefab;  // Prefab containing Name Text and Kick Button

    private NetworkList<FixedString64Bytes> playerNames = new NetworkList<FixedString64Bytes>();
    private Dictionary<ulong, string> clientNamesMap = new Dictionary<ulong, string>();

    private void Awake() => Instance = this;

    public override void OnNetworkSpawn()
    {
        playerNames.OnListChanged += (e) => UpdateStatusUI();

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
        }
        UpdateStatusUI();
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
        }
    }

    private void UpdateStatusUI()
    {
        // ডাইনামিক লিস্ট রিফ্রেশ (Todo App Style)
        foreach (Transform child in playerListContainer) Destroy(child.gameObject);

        for (int i = 0; i < playerNames.Count; i++)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer);
            string pName = playerNames[i].ToString();
            entry.GetComponentInChildren<TMP_Text>().text = pName;

            Button kickBtn = entry.GetComponentInChildren<Button>();
            if (IsServer)
            {
                ulong targetId = GetIdFromName(pName);
                if (targetId == NetworkManager.Singleton.LocalClientId) kickBtn.gameObject.SetActive(false);
                else kickBtn.onClick.AddListener(() => KickPlayer(targetId));
            }
            else kickBtn.gameObject.SetActive(false);
        }

        if (IsServer) startGameButton.interactable = playerNames.Count >= 2;
    }

    private ulong GetIdFromName(string name)
    {
        foreach (var pair in clientNamesMap) if (pair.Value == name) return pair.Key;
        return 999;
    }

    public void KickPlayer(ulong clientId)
    {
        if (!IsServer) return;
        NotifyKickedClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { clientId } } });
        StartCoroutine(DisconnectDelay(clientId));
    }

    [ClientRpc]
    private void NotifyKickedClientRpc(ClientRpcParams rpcParams = default)
    {
        LocalNetworkUI.Instance.ShowKickMessage("You have been kicked from the lobby!");
    }

    private System.Collections.IEnumerator DisconnectDelay(ulong clientId)
    {
        yield return new WaitForSeconds(0.5f);
        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    public void StartGame()
    {
        if (!IsServer) return;
        uiPanel.SetActive(false);
        HideUIClientRpc();

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            GameObject playerInstance = Instantiate(playerPrefab);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            if (clientNamesMap.TryGetValue(clientId, out string name))
                playerInstance.GetComponent<PlayerNameDisplay>().SetPlayerName(name);
        }
    }

    [ClientRpc] private void HideUIClientRpc() => uiPanel.SetActive(false);
}