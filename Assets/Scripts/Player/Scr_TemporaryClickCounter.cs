using UnityEngine;
using TMPro;

public class Scr_TemporaryClickCounter : MonoBehaviour
{
    public int clickCount = 0;
    public TMP_Text text;

    public void IncrementClickCounter()
    {
        clickCount++;
        text.text = clickCount.ToString();
    }
}
