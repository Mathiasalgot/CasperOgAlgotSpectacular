using UnityEngine;

public class Scr_WindowPortal : MonoBehaviour
{
    public Material masterMaterial;
    public float timeToOpen = 1.5f;

    private bool open = false;
    private Vector2 openPosition;
    private Vector2 mousePosition;

    private void Update()
    {

      
       // masterMaterial.SetFloat("_Dialation", Mathf.Sin(Time.time));
       // masterMaterial.SetVector("_Position", new Vector2(Screen.width, Screen.height) * 0.5f);

        if (open)
        {
            masterMaterial.SetFloat("_Dialation", Mathf.Lerp(masterMaterial.GetFloat("_Dialation"), 1, Time.deltaTime * 16));
            //shouldOpen = false; // Reset after opening
            if(Vector2.Distance(mousePosition, openPosition) > 150) 
            { 
                open = false; // Close if mouse moves too far away
                masterMaterial.SetFloat("_Dialation", 0); // Reset dilation
            }
        }
    }

    public void OpenPortal(Vector2 portalPosition)
    {
        Debug.Log("Open Portal 1");
        masterMaterial.SetVector("_Position", portalPosition);
        openPosition = portalPosition;
        open = true;
    }

    public void UpdateMousePosition(Vector2 mousePos)
    {
        mousePosition = mousePos;
    }
}
