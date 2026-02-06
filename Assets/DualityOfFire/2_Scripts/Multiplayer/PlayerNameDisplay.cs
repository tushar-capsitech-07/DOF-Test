using Unity.Netcode;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : NetworkBehaviour
{
    public TMP_Text nameTagText;
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += (oldV, newV) => nameTagText.text = newV.ToString();
        nameTagText.text = playerName.Value.ToString();
    }

    public void SetPlayerName(string name) { if (IsServer) playerName.Value = name; }

    private void LateUpdate()
    {
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}