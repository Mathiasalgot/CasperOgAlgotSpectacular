using UnityEngine;
namespace MA.Events
{
    public interface IGameEventListener<T>
    {
        void OnEventRaised(T item);
        GameObject GetGameobject();
    }
}