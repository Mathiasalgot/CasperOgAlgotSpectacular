using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scr_ChatMessagePopup : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject messageParent;
    public GameObject imageParent;
    public TMP_Text textField;
    public RawImage imageField;

    public Transform Owner { get; private set; }
    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void InitAsMessage(string message, Transform owner)
    {
        ResetVisuals();

        textField.text = message;
        messageParent.SetActive(true);

        Owner = owner;
    }

    public void InitAsImage(Texture2D image, Transform owner)
    {
        ResetVisuals();

        imageField.texture = image;
        imageParent.SetActive(true);

        Owner = owner;
    }

    private void ResetVisuals()
    {
        messageParent.SetActive(false);
        imageParent.SetActive(false);
    }
}
