using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    public GameObject object1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (IsServer)
            {
                SpawnObjectServer();
            }
            else
            {
                SpawnObjectServerRpc();
            }
            
        }
    }
    void SpawnObjectServer()
    {
        GameObject clone = Instantiate(object1, transform.position + new Vector3(0,3,0), Quaternion.identity);
        clone.GetComponent<NetworkObject>().Spawn();
    }

    [ServerRpc]
    void SpawnObjectServerRpc()
    {
        SpawnObjectServer();
    }
}
