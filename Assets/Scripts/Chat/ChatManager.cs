using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;
    void Awake()
    { ChatManager.Singleton = this; }



    public Scr_ChatMessagePopup messagePrefab;
    public Transform chatContent;
    //[SerializeField] CanvasGroup chatContent;
    [SerializeField] TMP_InputField chatInput;

    public string playerName;
    

    //To be replaced by new Input System
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage(chatInput.text, playerName);
            chatInput.text = "";
        }
    }

    #region Output Functions

    //Both functions instantiate the same prefab, but with different initializations

    void AddMessage(string msg)
    {
        Instantiate<Scr_ChatMessagePopup>(messagePrefab, chatContent).InitAsMessage();
    }

    void AddImage(Texture2D image)
    {
        Instantiate<Scr_ChatMessagePopup>(messagePrefab, chatContent).InitAsImage(image);

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
    public void SendChatMessage(string _message, string _fromWho = null)
    {
        if (string.IsNullOrWhiteSpace(_message)) return;

        string S = _fromWho + " > " + _message;
        SendChatMessageServerRpc(S);
    }

        #region Chat Server Functions


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

        #endregion

    #endregion


}