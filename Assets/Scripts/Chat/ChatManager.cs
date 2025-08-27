using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;
    void Awake()
    { ChatManager.Singleton = this; }



    public Scr_ScreenspaceChatHandler chatHandler;
    //[SerializeField] CanvasGroup chatContent;


    public string playerName;


    #region Output Functions

    //Both functions instantiate the same prefab, but with different initializations

    void AddMessage(string msg, ulong owner)
    {
        chatHandler.SpawnMessage(msg, NetworkManager.ConnectedClients[owner].PlayerObject.transform);
        //Instantiate<Scr_ChatMessagePopup>(messagePrefab, chatContent).InitAsMessage(msg, NetworkManager.ConnectedClients[owner].PlayerObject.transform);
    }

    void AddImage(Texture2D image, ulong owner)
    {
        chatHandler.SpawnImage(image, NetworkManager.ConnectedClients[owner].PlayerObject.transform);
        //Instantiate<Scr_ChatMessagePopup>(messagePrefab, chatContent).InitAsImage(image, NetworkManager.ConnectedClients[owner].PlayerObject.transform);
    }
    #endregion

    #region Image Functions

    //Base public function to send an image message
    public void SendImageMessage(Texture2D image)
    {
        SendChatImageServerRpc(CompressImage(image));
    }

        #region Image Server Functions

        [ServerRpc(RequireOwnership = false)]
        void SendChatImageServerRpc(byte[] image, ServerRpcParams rpcParams = default)
        {
            ReceiveImageMessageClientRpc(image, rpcParams.Receive.SenderClientId);
        }

        [ClientRpc]
        void ReceiveImageMessageClientRpc(byte[] image, ulong ownerId)
        {
            Texture2D recievedTexture = new Texture2D(128, 128, TextureFormat.RGB24, false);
            recievedTexture.LoadRawTextureData(image);
            recievedTexture.Apply();
            ChatManager.Singleton.AddImage(recievedTexture, ownerId);
        }
        #endregion

        #region Image Helper Functions

        public byte[] CompressImage(Texture2D inImage)
        {
            Texture2D temp = Resize(inImage, 128, 128);

            //Converts the texture to a byte array and returns
            return temp.GetRawTextureData();
        }


        // Uses a RenderTexture as a workaround to copy the texture data to a Texture with a smaller resolution.
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
    #endregion

    #endregion

    #region Chat Functions

    //Base public function to send a chat message
    public void SendChatMessage(string _message)
    {
        if (string.IsNullOrWhiteSpace(_message)) return;

        SendChatMessageServerRpc(_message);
    }

        #region Chat Server Functions


        [ServerRpc(RequireOwnership = false)]
        void SendChatMessageServerRpc(string message, ServerRpcParams rpcParams = default)
        {
            ReceiveChatMessageClientRpc(message, rpcParams.Receive.SenderClientId);
        }

        [ClientRpc]
        void ReceiveChatMessageClientRpc(string message, ulong ownerId)
        {
            ChatManager.Singleton.AddMessage(ownerId.ToString() + ": " + message, ownerId);
        }

        #endregion

    #endregion


}