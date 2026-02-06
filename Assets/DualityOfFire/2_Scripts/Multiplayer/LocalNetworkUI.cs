using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class LocalNetworkUI : MonoBehaviour
{
    public static LocalNetworkUI Instance;

    [Header("UI References")]
    public GameObject uiPanel;
    public TMP_InputField joinCodeInput;
    public TMP_InputField nameInput;
    public TMP_Text discoveryStatusText;
    public TMP_Text hostCodeText;  
    public GameObject hostButton, clientButton, startGameButton;

    public DiscoveryHandler discoveryHandler;
    private UnityTransport transport;

    private void Awake() => Instance = this;

    private void Start()
    {
        transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        NetworkManager.Singleton.OnClientConnectedCallback += OnConnected;
        discoveryHandler.StartListening((ip) => { joinCodeInput.text = ip; discoveryStatusText.text = "Game Found!"; });
    }

    //public void StartHost()
    //{
    //    if (string.IsNullOrEmpty(nameInput.text)) return;
    //    string ip = LocalIPFinder.GetLocalIPv4();
    //    transport.SetConnectionData(ip, 7777);
    //    NetworkManager.Singleton.StartHost();
    //    discoveryHandler.StartBroadcasting(ip);
    //    UpdateUI(true);
    //}

    public void StartHost()
    {
        if (string.IsNullOrEmpty(nameInput.text)) return;

        string hostIP = LocalIPFinder.GetLocalIPv4();

        if (string.IsNullOrEmpty(hostIP) || hostIP == "127.0.0.1")
        {
            hostIP = "192.168.43.1";
        }

        transport.SetConnectionData(hostIP, 7777);
        NetworkManager.Singleton.StartHost();
        discoveryHandler.StartBroadcasting(hostIP);

        hostCodeText.text = "HOSTING ON:\n" + hostIP;
        UpdateUI(true);
    }

    public void StartClient()
    {
        if (string.IsNullOrEmpty(nameInput.text) || string.IsNullOrEmpty(joinCodeInput.text)) return;
        transport.SetConnectionData(joinCodeInput.text, 7777);
        NetworkManager.Singleton.StartClient();
        UpdateUI(true);
    }

    public void ShowKickMessage(string msg)
    {
        discoveryStatusText.text = "<color=red>" + msg + "</color>";
        UpdateUI(false);
    }

    private void UpdateUI(bool connected)
    {
        hostButton.SetActive(!connected);
        clientButton.SetActive(!connected);
        nameInput.interactable = !connected;
        joinCodeInput.interactable = !connected;
        uiPanel.SetActive(!connected || LobbyManager.Instance.uiPanel.activeSelf);
    }

    private void OnConnected(ulong id)
    {
        if (id == NetworkManager.Singleton.LocalClientId)
            LobbyManager.Instance.AddPlayerNameServerRpc(nameInput.text);
    }
}