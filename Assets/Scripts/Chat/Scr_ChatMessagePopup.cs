using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scr_ChatMessagePopup : MonoBehaviour
{
    public TMP_Text textField;
    public RawImage imageField;
    public void InitAsMessage()
    {

    }

    public void InitAsImage(Texture2D image)
    {
        imageField.texture = image;
    }
}
