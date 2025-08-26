using System.Collections.Generic;
using UnityEngine;

public class Scr_ScreenspaceChatHandler : MonoBehaviour
{
    [Header("References")]
    public Camera sceneCamera; // The main camera in your scene
    public RectTransform chatCanvas; // The canvas (must be Screen Space - Overlay or Camera)
    public Scr_ChatMessagePopup messagePrefab;

    private List<Scr_ChatMessagePopup> activeMessages = new List<Scr_ChatMessagePopup>();

    public void SpawnMessage(string text, Transform owner)
    {
        var instance = Instantiate(messagePrefab, chatCanvas);
        instance.InitAsMessage(text, owner);
        activeMessages.Add(instance);
        Destroy(instance.gameObject, 5f); // temp lifetime control
    }

    public void SpawnImage(Texture2D tex, Transform owner)
    {
        var instance = Instantiate(messagePrefab, chatCanvas);
        instance.InitAsImage(tex, owner);
        activeMessages.Add(instance);
        Destroy(instance.gameObject, 5f);
    }

    private void Update()
    {
        // Update screen positions for all active messages
        foreach (var msg in activeMessages)
        {
            if (msg == null) continue;
            if (msg.Owner == null) continue;

            Vector3 screenPos = sceneCamera.WorldToScreenPoint(msg.Owner.position + Vector3.up * 1.5f);
            msg.RectTransform.position = screenPos;
        }
    }
}
