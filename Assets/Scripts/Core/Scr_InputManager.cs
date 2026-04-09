using UnityEngine;
using UnityEngine.InputSystem;
using MA.Events;

public class Scr_InputManager : MonoBehaviour
{
    //Local variable storage
    private bool jumpPressed;
    private Vector2 mouseDelta;
    private Vector2 moveInput;

    //Output Events
    public VoidEvent jumpEvent;
    public Vector2Event movementEvent;
    public Vector2Event mouseDeltaEvent;

    public void GetMouseDelta(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            mouseDelta = context.ReadValue<Vector2>();
            Debug.Log("mouse started moving");
        }
        if (context.performed)
        {
            mouseDelta = context.ReadValue<Vector2>();
        }
        if (context.canceled)
        {
            mouseDelta = Vector2.zero;
        }

        mouseDeltaEvent.Raise(mouseDelta);
    }
    public void GetWalkInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            moveInput = context.ReadValue<Vector2>();
            movementEvent.Raise(moveInput);
        }
        if (context.canceled)
        {
            movementEvent.Raise(Vector2.zero);
        }
    }
    public void GetJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            jumpPressed = true;
            jumpEvent.Raise();
        }
        if (context.canceled)
        {
            jumpPressed = false;
        }
    }

}
