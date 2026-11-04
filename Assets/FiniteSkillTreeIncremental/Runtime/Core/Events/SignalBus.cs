using System;
using System.Collections.Generic;

namespace FiniteSkillTreeIncremental.Core.Events
{
    /// <summary>
    /// Lightweight, type-safe decoupled message bus for connection points across subsystems.
    /// </summary>
    public static class SignalBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _subscribers[type] = list;
            }
            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
                if (list.Count == 0)
                {
                    _subscribers.Remove(type);
                }
            }
        }

        public static void Publish<T>(T signal)
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                // Create a temporary copy to avoid mutation during iteration
                var copy = list.ToArray();
                for (int i = 0; i < copy.Length; i++)
                {
                    if (copy[i] is Action<T> action)
                    {
                        try
                        {
                            action.Invoke(signal);
                        }
                        catch (Exception ex)
                        {
                            UnityEngine.Debug.LogException(ex);
                        }
                    }
                }
            }
        }

        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }
}
