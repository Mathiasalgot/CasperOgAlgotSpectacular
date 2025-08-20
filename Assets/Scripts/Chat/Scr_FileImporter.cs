using B83.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.IO;

public class Scr_FileImporter : MonoBehaviour
{
    public UnityEvent<Texture2D> onImageImported;

    void OnEnable()
    {
        // must be installed on the main thread to get the right thread id.
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }
    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    async void OnFiles(List<string> aFiles, POINT aPos)
    {
        // do something with the dropped file names. aPos will contain the 
        // mouse position within the window where the files has been dropped.
        string str = aFiles.Aggregate((a, b) => a + "\n\t" + b);
        if (str.EndsWith(".png") || str.EndsWith(".jpg") || str.EndsWith(".jpeg"))
        {
            Texture2D tex = await GetTextures(str);
            onImageImported.Invoke(tex);
        }
        else
        {
            Debug.Log(str + " is not a readable image format");
        }

    }

  

    public async Task<Texture2D> GetTextures(string path)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture("file:///" + path);
        await www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log(www.error);
            return null;
        }
        else
        {
            Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            //Sprite mySprite = Sprite.Create(myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(myTexture.width / 2, myTexture.height / 2));
            return myTexture;
        }
        
    }


}
