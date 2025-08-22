using UnityEngine;
using UnityEngine.Events;


namespace MA.Events
{
    public abstract class ExpandedBoolEventListener<T, E, UER> : MonoBehaviour,
        IGameEventListener<bool> where E : BaseGameEvent<bool> where UER : UnityEvent<bool>
    {
        [SerializeField] private E gameEvent;
        public E GameEvent { get { return gameEvent; } set { gameEvent = value; } }

        [SerializeField] private UER trueEventResponse;
        [SerializeField] private UER falseEventResponse;
        [SerializeField] private UER defaultEventResponse;

        private void OnEnable()
        {
            if(gameEvent == null) { return; }

            GameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if(gameEvent == null) { return; }

            GameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(bool item)
        {
            if (item)
            {
                if (trueEventResponse != null)
                {
                    trueEventResponse.Invoke(true);
                }
            }
            else
            {
                if (falseEventResponse != null)
                {
                    falseEventResponse.Invoke(false);
                }
            }

            if (defaultEventResponse != null)
            {
                defaultEventResponse.Invoke(item);
            }

        }

        public GameObject GetGameobject()
        {
            return this.gameObject;
        }
    }
}



