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
    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void InitAsMessage(string message)
    {
        ResetVisuals();

        textField.text = message;
        messageParent.SetActive(true);

    }

    public void InitAsImage(Texture2D image)
    {
        ResetVisuals();

        imageField.texture = image;
        imageParent.SetActive(true);

    }

    private void ResetVisuals()
    {
        messageParent.SetActive(false);
        imageParent.SetActive(false);
    }
}
