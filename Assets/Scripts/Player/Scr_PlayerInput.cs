using UnityEngine;
using Unity.Netcode;

public class Scr_PlayerInput : NetworkBehaviour
{
    [Header("References")]
    public Scr_CharacterMovement playerMovement;
    public Transform cameraTransform;

    /*[SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private float distance = 5.0f;
    [SerializeField] private float minPitch = -20f;
    [SerializeField] private float maxPitch = 80f;

    private float _yaw = 0f;
    private float _pitch = 0f;*/

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            
            cameraTransform.gameObject.SetActive(true);

            
        }
        
    }

    public void HandleMovementInput(Vector2 rawInput)
    {
        if (!IsOwner || cameraTransform == null) return;
        
        if (rawInput == Vector2.zero)
        {
            playerMovement.SetMovement(rawInput);
            return;
        }

        rawInput.Normalize();

        // Get camera directions and flatten them on the Y axis
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Translate the raw input into camera-relative world space
        Vector3 moveDirection = (camForward * rawInput.y) + (camRight * rawInput.x);

        // Convert the 3D direction back into a Vector2 (X and Z) for the movement script
        Vector2 finalMoveInput = new Vector2(moveDirection.x, moveDirection.z);

        // Pass the translated input to the movement script
        playerMovement.SetMovement(finalMoveInput);
        
        
    }

    public void HandleJumpInput()
    {
        if (IsOwner)
        {
            playerMovement.Jump();
        }
        

    }
    /*public void HandleMouseInput(Vector2 mouseInput)
    {
        if (IsOwner)
        {
            // 1. Accumulate input and apply sensitivity
            _yaw += mouseInput.x * rotationSpeed;
            _pitch -= mouseInput.y * rotationSpeed; // Inverted typical for natural feel

            // 2. Clamp the pitch so the camera doesn't flip over the top
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            // 3. Convert Euler angles to a Quaternion rotation
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);

            // 4. Calculate the new position
            // We take the forward vector of that rotation, move back by 'distance'
            Vector3 offset = rotation * new Vector3(0, 0, -distance);

            // 5. Update Camera position and look at the player
            cameraTransform.position = transform.position + offset;
            cameraTransform.LookAt(transform.position + Vector3.up * 1.5f); // Offset up to look at head/shoulders
        }
    }*/
}