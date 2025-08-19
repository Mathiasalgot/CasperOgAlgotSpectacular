using Unity.Netcode;
using UnityEngine;

public class PlayerScript : NetworkBehaviour
{
    public GameObject object1;


    //float moveHorizontal, moveForward; måske bare fjern det her
    public float movSpeed = 10;
    public float stoppingDistance = 0.1f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    Rigidbody rb;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!IsOwner)
        {
            return;
        }
        //moveHorizontal = Input.GetAxisRaw("Horizontal");
        //moveForward = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.C))
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
        //Prototype jump(skal nok slettes helt)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x,3,rb.linearVelocity.z);
        }


        // Inside Update
        if (Input.GetMouseButtonDown(0) && IsOwner)
        {
            SetTargetPosition();
        }

        if (isMoving && IsOwner) // Only owner moves itself
        {
            MoveCharacter();
        }

    }
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        //OldPlayerMovement();
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
    
    void SetTargetPosition()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            targetPosition = hit.point + new Vector3(0,0.5f,0);
            isMoving = true;
        }
    }
    void MoveCharacter()
    {
        // Direction toward target
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move character
        transform.position += direction * movSpeed * Time.deltaTime;

        // Optional: rotate to face movement direction
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10f * Time.deltaTime);
        }

        // Stop when close enough
        if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
        {
            isMoving = false;
        }
    }
    /*
        void OldPlayerMovement()
        {

            Vector3 movement = (Vector3.right * moveHorizontal + Vector3.forward * moveForward).normalized;
            Vector3 targetVelocity = movement * movSpeed;

            // apply movement to rb:
            Vector3 velocity = rb.linearVelocity;
            velocity.x = targetVelocity.x;
            velocity.z = targetVelocity.z;
            rb.linearVelocity = velocity;

            // if we arent moving and are on the ground, stop velocity so we dont slide:
            if (moveHorizontal == 0 && moveForward == 0)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

            }
        }*/
}
