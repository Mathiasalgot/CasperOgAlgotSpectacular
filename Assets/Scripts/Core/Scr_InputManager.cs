using UnityEngine;
using UnityEngine.InputSystem;
using MA.Events;

public class Scr_InputManager : MonoBehaviour
{
    //Local variable storage
    private bool primaryActionPressed;
    private Vector2 mouseDelta;
    private Vector2 mousePos;

    //Output Events
    public Vector2Event openPortalEvent;
    public Vector2Event primaryActionEvent;
    public Vector2Event mousePositionEvent;

    public void GetMouseDelta(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            mouseDelta = context.ReadValue<Vector2>();
        }
        if (context.performed)
        {
            mouseDelta = context.ReadValue<Vector2>();
        }
        if (context.canceled)
        {
            mouseDelta = Vector2.zero;
        }
    }
    public void GetMousePosition(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            mousePos = context.ReadValue<Vector2>();
            mousePositionEvent.Raise(mousePos);
        }
    }
    public void GetPrimaryAction(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            primaryActionPressed = true;
            primaryActionEvent.Raise(mousePos);
        }
        if (context.canceled)
        {
            primaryActionPressed = false;
        }
    }

    public void GetOpenPortal(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            openPortalEvent.Raise(mousePos);
        }
    }
}
