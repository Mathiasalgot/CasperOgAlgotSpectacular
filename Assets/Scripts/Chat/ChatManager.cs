using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.IO;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;

    public Scr_ChatMessagePopup messagePrefab;
    public Transform chatContent;
    //[SerializeField] CanvasGroup chatContent;
    [SerializeField] TMP_InputField chatInput;

    public Texture2D demoImage;

    public Texture2D debugImage;

    public string playerName;

    void Awake()
    { ChatManager.Singleton = this; }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text, playerName);
            chatInput.text = "";
        }
    }

    [ContextMenu("Send Demo Image")]
    public void SendDemoImage()
    {
        SendImageMessage(demoImage, playerName);
    }

    public void SendImageMessage(Texture2D image, string senderName)
    {

        SendChatImageServerRpc(CompressImage(image));
    }

    public byte[] CompressImage(Texture2D inImage)
    {
        Texture2D temp = Resize(inImage, 128, 128);
        //Graphics.CopyTexture(inImage, temp);
        //temp.Reinitialize(Mathf.Max(128), Mathf.Max(128), inImage.format, false);
        //temp.Apply();
        debugImage = temp;
        //return temp.GetRawTextureData();
        return temp.GetRawTextureData();
    }

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    public void SendChatMessage(string _message, string _fromWho = null)
    {
        if (string.IsNullOrWhiteSpace(_message)) return;

        string S = _fromWho + " > " + _message;
        SendChatMessageServerRpc(S);
    }

    void AddMessage(string msg)
    {
        Instantiate<Scr_ChatMessagePopup>(messagePrefab,chatContent).InitAsMessage();
    }

    void AddImage(Texture2D image)
    {
        Instantiate<Scr_ChatMessagePopup>(messagePrefab, chatContent).InitAsImage(image);

    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(string message)
    {
        ReceiveChatMessageClientRpc(message);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message)
    {
        ChatManager.Singleton.AddMessage(message);
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatImageServerRpc(byte[] image)
    {
        ReceiveImageMessageClientRpc(image);
    }

    [ClientRpc]
    void ReceiveImageMessageClientRpc(byte[] image)
    {
        Texture2D recievedTexture = new Texture2D(128, 128, TextureFormat.RGB24, false);
        recievedTexture.LoadRawTextureData(image);
        recievedTexture.Apply();
        ChatManager.Singleton.AddImage(recievedTexture);
    }
}