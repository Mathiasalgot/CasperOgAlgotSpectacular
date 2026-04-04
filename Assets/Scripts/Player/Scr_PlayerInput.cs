using UnityEngine;

public class Scr_PlayerInput : MonoBehaviour
{
    [Header("References")]
    public Scr_CharacterMovement playerMovement;
    public Transform cameraTransform;

    public void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    public void HandleMovementInput(Vector2 rawInput)
    {
        if(rawInput == Vector2.zero)
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

      playerMovement.Jump();

    }
}