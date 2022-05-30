using System;
using System.Collections.Generic;
using BilliardGame.EventHandlers;
using UnityEngine;

namespace BilliardGame.Managers
{
    public static class EventManager
    {
        private static Dictionary<string, Action<object, IGameEvent>> _subscribers = new Dictionary<string, Action<object, IGameEvent>>();

        public static void Subscribe(string eventID, Action<object, IGameEvent> callback)
        {
            if (_subscribers.ContainsKey(eventID))
                _subscribers[eventID] += callback;
            else
                _subscribers.Add(eventID, callback);
        }

        public static void Unsubscribe(string eventID, Action<object, IGameEvent> callback)
        {
            if (_subscribers.ContainsKey(eventID))
                _subscribers[eventID] -= callback;
        }

        public static void Notify(string eventID, object sender, IGameEvent gameEvent)
        {
            if (_subscribers.ContainsKey(eventID))
            {
                Action<object, IGameEvent> selectedCallback = _subscribers[eventID];
                Debug.Assert(selectedCallback != null, "There are no subscribed events for this " + eventID);

                selectedCallback(sender, gameEvent);
            }
        }
    }
}
