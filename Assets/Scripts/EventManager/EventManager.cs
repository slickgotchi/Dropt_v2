using System;
using System.Collections.Generic;
using Unity.Netcode;

public class EventManager : NetworkBehaviour
{
    // Singleton instance
    public static EventManager Instance;

    // Dictionary to store events with string as the key and Action (event) as the value
    private Dictionary<string, Action> m_eventDictionary;

    // Dictionary to store events with parameters (using Action<T>)
    private Dictionary<string, Action<object>> m_eventDictionaryWithParams;

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of the AudioManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        m_eventDictionary = new Dictionary<string, Action>();
        m_eventDictionaryWithParams = new Dictionary<string, Action<object>>();
    }

    #region Event without parameters

    // Subscribe to an event
    public void StartListening(string eventName, Action listener)
    {
        if (m_eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent += listener;
            m_eventDictionary[eventName] = thisEvent;
        }
        else
        {
            m_eventDictionary.Add(eventName, listener);
        }
    }

    // Unsubscribe from an event
    public void StopListening(string eventName, Action listener)
    {
        if (m_eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
                m_eventDictionary.Remove(eventName);
            else
                m_eventDictionary[eventName] = thisEvent;
        }
    }

    // Trigger the event
    public void TriggerEvent(string eventName)
    {
        if (m_eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            thisEvent.Invoke();
        }
    }

    #endregion

    #region Event with parameters

    // Subscribe to an event with a parameter
    public void StartListeningWithParam(string eventName, Action<object> listener)
    {
        if (m_eventDictionaryWithParams.TryGetValue(eventName, out Action<object> thisEvent))
        {
            thisEvent += listener;
            m_eventDictionaryWithParams[eventName] = thisEvent;
        }
        else
        {
            m_eventDictionaryWithParams.Add(eventName, listener);
        }
    }

    // Unsubscribe from an event with a parameter
    public void StopListeningWithParam(string eventName, Action<object> listener)
    {
        if (m_eventDictionaryWithParams.TryGetValue(eventName, out Action<object> thisEvent))
        {
            thisEvent -= listener;
            if (thisEvent == null)
                m_eventDictionaryWithParams.Remove(eventName);
            else
                m_eventDictionaryWithParams[eventName] = thisEvent;
        }
    }

    // Trigger the event with a parameter
    public void TriggerEventWithParam(string eventName, object param)
    {
        if (m_eventDictionaryWithParams.TryGetValue(eventName, out Action<object> thisEvent))
        {
            thisEvent.Invoke(param);
        }
    }

    #endregion
}
