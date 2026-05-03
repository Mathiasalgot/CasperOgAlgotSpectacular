using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class Scr_NetworkPlayerInit : NetworkBehaviour
{
    public List<GameObject> localPlayerComponents;
    private void Start()
    {
        if (!IsOwner)
        {
            Scr_PlayerInput input = GetComponent<Scr_PlayerInput>();

            if (input)
            {
                Destroy(input);
            }

            foreach (GameObject component in localPlayerComponents)
            {
                Destroy(component);
            }
        }

       
    }
}
