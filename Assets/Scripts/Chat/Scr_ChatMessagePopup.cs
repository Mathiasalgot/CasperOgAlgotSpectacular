using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scr_ChatMessagePopup : MonoBehaviour
{
    public GameObject messageParent;
    public GameObject imageParent;
    public TMP_Text textField;
    public RawImage imageField;

    private bool init;
    private Transform myOwner;
    public void InitAsMessage(string message, Transform owner)
    {
        textField.text = message;
        messageParent.SetActive(true);
        StartDestroyTimer();

        init = true;
        myOwner = owner; 
    }

    public void InitAsImage(Texture2D image, Transform owner)
    {
        imageField.texture = image;
        imageParent.SetActive(true);
        StartDestroyTimer();

        init = true;
        myOwner = owner;
    }

    private void Update()
    {
        transform.position = myOwner.position + Vector3.one * 1.5f;
    }

    void StartDestroyTimer()
    {
        Destroy(gameObject, 5f); // Destroys the popup after 5 seconds
    }
}
