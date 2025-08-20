using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scr_ChatMessagePopup : MonoBehaviour
{
    public GameObject messageParent;
    public GameObject imageParent;
    public TMP_Text textField;
    public RawImage imageField;
    public void InitAsMessage(string message)
    {
        textField.text = message;
        messageParent.SetActive(true);
        StartDestroyTimer();

    }

    public void InitAsImage(Texture2D image)
    {
        imageField.texture = image;
        imageParent.SetActive(true);
        StartDestroyTimer();
    }

    void StartDestroyTimer()
    {
        Destroy(gameObject, 5f); // Destroys the popup after 5 seconds
    }
}
