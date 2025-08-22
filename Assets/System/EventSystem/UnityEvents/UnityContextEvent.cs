using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MA.Events
{
    [System.Serializable] public class UnityContextEvent : UnityEvent<InputAction.CallbackContext> { }
}