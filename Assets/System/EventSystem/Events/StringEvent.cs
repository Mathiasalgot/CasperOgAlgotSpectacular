using UnityEngine;

namespace MA.Events
{
    [CreateAssetMenu(fileName = "New String Event", menuName = "Game Events/String Event")]
    public class StringEvent : BaseGameEvent<string> { }
}