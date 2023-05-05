/**
 * Danceology
 * Originally Developed by Team Danceology Spring 2023
 * Christine Jung, Xiaoying Meng, Jiacheng Qiu, Yiming Xiao, Xueying Yang, Angela Zhang
 * 
 * This script and all related assets fall under the CC BY-NC-SA 4.0 License
 * All future derivations of this code should contain the above attribution
 **/

using System;
using System.Collections.Generic;

public delegate void CallBack();
public delegate void CallBack<T>(T arg);
public delegate void CallBack<T, X>(T arg1, X arg2);
public delegate void CallBack<T, X, Y>(T arg1, X arg2, Y arg3);

/// <summary>
/// High-level event management class
/// </summary>
public class EventBus
{
    private static Dictionary<EventTypes, Delegate> eventDictionary = new Dictionary<EventTypes, Delegate>();

    private static void OnListenerAdding(EventTypes eventType, Delegate callBack)
    {
        if (!eventDictionary.ContainsKey(eventType))
        {
            eventDictionary.Add(eventType, null);
        }

        Delegate d = eventDictionary[eventType];
        if (d != null && d.GetType() != callBack.GetType())
        {
            throw new Exception(string.Format("Callback for event {0} has incorrect type: current entry has {1}, trying to insert {2}", eventType, d.GetType(), callBack.GetType()));
        }
    }

    private static bool OnListenerRemoving(EventTypes eventType, Delegate callBack)
    {
        if (eventDictionary.ContainsKey(eventType))
        {
            Delegate d = eventDictionary[eventType];
            if (d == null)
            {
                return false;
            }
            else if (d.GetType() != callBack.GetType())
            {
                throw new Exception(string.Format("Callback for event {0} has incorrect type: current entry has {1}, trying to insert {2}", eventType, d.GetType(), callBack.GetType()));
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    #region AddListener
    public static void AddListener(EventTypes eventType, CallBack callBack)
    {
        OnListenerAdding(eventType, callBack);
        eventDictionary[eventType] = (CallBack)eventDictionary[eventType] + callBack;
    }

    public static void AddListener<T>(EventTypes eventType, CallBack<T> callBack)
    {
        OnListenerAdding(eventType, callBack);
        eventDictionary[eventType] = (CallBack<T>)eventDictionary[eventType] + callBack;
    }

    public static void AddListener<T, X>(EventTypes eventType, CallBack<T, X> callBack)
    {
        OnListenerAdding(eventType, callBack);
        eventDictionary[eventType] = (CallBack<T, X>)eventDictionary[eventType] + callBack;
    }

    public static void AddListener<T, X, Y>(EventTypes eventType, CallBack<T, X, Y> callBack)
    {
        OnListenerAdding(eventType, callBack);
        eventDictionary[eventType] = (CallBack<T, X, Y>)eventDictionary[eventType] + callBack;
    }
    #endregion

    #region RemoveListener
    public static void RemoveListener(EventTypes eventType, CallBack callBack)
    {
        if (!OnListenerRemoving(eventType, callBack)) return;
        eventDictionary[eventType] = (CallBack)eventDictionary[eventType] - callBack;
    }

    public static void RemoveListener<T>(EventTypes eventType, CallBack<T> callBack)
    {
        if (!OnListenerRemoving(eventType, callBack)) return;
        eventDictionary[eventType] = (CallBack<T>)eventDictionary[eventType] - callBack;
    }

    public static void RemoveListener<T, X>(EventTypes eventType, CallBack<T, X> callBack)
    {
        if (!OnListenerRemoving(eventType, callBack)) return;
        eventDictionary[eventType] = (CallBack<T, X>)eventDictionary[eventType] - callBack;
    }

    public static void RemoveListener<T, X, Y>(EventTypes eventType, CallBack<T, X, Y> callBack)
    {
        if (!OnListenerRemoving(eventType, callBack)) return;
        eventDictionary[eventType] = (CallBack<T, X, Y>)eventDictionary[eventType] - callBack;
    }
    #endregion

    #region Broadcast
    public static void Broadcast(EventTypes eventType)
    {
        if (eventDictionary.TryGetValue(eventType, out Delegate d))
        {
            if (d == null) return;

            CallBack callBack = (CallBack)d;
            if (callBack != null)
            {
                callBack();
            }
        }
    }

    public static void Broadcast<T>(EventTypes eventType, T arg)
    {
        if (eventDictionary.TryGetValue(eventType, out Delegate d))
        {
            if (d == null) return;

            CallBack<T> callBack = (CallBack<T>)d;
            if (callBack != null)
            {
                callBack(arg);
            }
        }
    }

    public static void Broadcast<T, X>(EventTypes eventType, T arg0, X arg1)
    {
        if (eventDictionary.TryGetValue(eventType, out Delegate d))
        {
            if (d == null) return;

            CallBack<T, X> callBack = (CallBack<T, X>)d;
            if (callBack != null)
            {
                callBack(arg0, arg1);
            }
        }
    }

    public static void Broadcast<T, X, Y>(EventTypes eventType, T arg0, X arg1, Y arg2)
    {
        if (eventDictionary.TryGetValue(eventType, out Delegate d))
        {
            if (d == null) return;

            CallBack<T, X, Y> callBack = (CallBack<T, X, Y>)d;
            if (callBack != null)
            {
                callBack(arg0, arg1, arg2);
            }
        }
    }
    #endregion
}
