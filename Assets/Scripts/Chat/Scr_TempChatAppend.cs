using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Scr_TempChatAppend : NetworkBehaviour
{

    public TMP_Text chatText;
    public void AppendTextToTMP(string inText)
    {
        if (IsServer)
        {
            SendTextServerRpc(inText);

        }
        else
        {
            RecieveTextClientRpc(inText);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void SendTextServerRpc(string inText)
    {
        RecieveTextClientRpc(inText);
    }

    [ClientRpc]
    public void RecieveTextClientRpc(string inText)
    {
        chatText.text = chatText.text + "\n" + inText;
    }
}
