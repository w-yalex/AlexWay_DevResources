using System;
using System.Collections.Generic;


namespace AW.UnityResources
{
    public static class EventBus
    {
        
        private static readonly Dictionary<Type, Delegate> _eventTable = new();

        public static void Subscribe<TData>(Action<TData> listener) 
            where TData : struct
        {
            if(_eventTable.TryGetValue(typeof(TData), out Delegate listeners))
                _eventTable[typeof(TData)] = (Action<TData>)listeners + listener;
            else _eventTable[typeof(TData)] = listener;

        }

        public static void Unsubscribe<TData>(Action<TData> listener)
            where TData : struct
        {
            if(_eventTable.TryGetValue(typeof(TData), out Delegate listeners))
            {
                _eventTable[typeof(TData)] = (Action<TData>)listeners - listener;
                if(_eventTable[typeof(TData)] == null) _eventTable.Remove(typeof(TData));
            }
        }


        public static void Trigger<TData>(TData eventData)
            where TData : struct
        {
            if(_eventTable.TryGetValue(typeof(TData), out Delegate listeners))
                ((Action<TData>)listeners)?.Invoke(eventData);

        }


        public static void ClearEvents() => _eventTable.Clear();
    }

}
