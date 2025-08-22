using UnityEngine;
using UnityEngine.InputSystem;


namespace MA.Events
{
    [CreateAssetMenu(fileName = "New Context Event", menuName = "Game Events/Context Event")]
    public class ContextEvent : BaseGameEvent<InputAction.CallbackContext> { }
} 