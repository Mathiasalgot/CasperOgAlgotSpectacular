using System.Collections.Generic;
using UnityEngine;

public class Scr_ScreenspaceChatHandler : MonoBehaviour
{
    [Header("References")]
    public Camera sceneCamera; // The main camera in your scene
    public RectTransform chatCanvas; // The canvas (must be Screen Space - Overlay or Camera)
    public Scr_ChatMessagePopup messagePrefab;

    [Header("Variables")]
    public float messageLifetime = 5f;
    public float messageUpdrift = 2f;

    private List<ChatMessageContainer> activeMessages = new List<ChatMessageContainer>();

    public class ChatMessageContainer
    {
        public Scr_ChatMessagePopup messageObject;
        public float lifetime;
        public Transform owner;

        public ChatMessageContainer(Scr_ChatMessagePopup msgObj, float life, Transform own)
        {
            messageObject = msgObj;
            lifetime = life;
            owner = own;
        }
    }

    public void SpawnMessage(string text, Transform owner)
    {
        var instance = Instantiate(messagePrefab, chatCanvas);
        instance.InitAsMessage(text);
        activeMessages.Add(new ChatMessageContainer(instance, messageLifetime, owner));
        Destroy(instance.gameObject, 5f); // temp lifetime control
    }

    public void SpawnImage(Texture2D tex, Transform owner)
    {
        var instance = Instantiate(messagePrefab, chatCanvas);
        instance.InitAsImage(tex);
        activeMessages.Add(new ChatMessageContainer(instance, messageLifetime, owner));
    }

    private void Update()
    {
        // Update screen positions for all active messages
        foreach (var msg in activeMessages)
        {
            if (msg.messageObject == null) continue;
            if (msg.owner == null) continue;


            Vector3 screenPos = sceneCamera.WorldToScreenPoint(msg.owner.position + Vector3.up * (1.5f + messageUpdrift - ( messageUpdrift * (msg.lifetime/messageLifetime))));
            msg.messageObject.RectTransform.position = screenPos;

            msg.lifetime -= Time.deltaTime;

            if (msg.lifetime <= 0)
            {
                Destroy(msg.messageObject.gameObject);
                Destroy(msg.messageObject);
                continue;
            }
        }
    }
}
