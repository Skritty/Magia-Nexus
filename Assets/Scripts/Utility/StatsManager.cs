using System;
using System.Collections.Generic;
using TwitchLib.Client.Models;
using UnityEngine;
public class StatsManager : MonoBehaviour
{
    private void OnDisable()
    {
        Stats.ClearAllStats();
        Modifier.ClearDurationModifiers();
    }

    public void FixedUpdate()
    {
        Modifier.Tick();
    }
}

public static class Stats
{
    private static Dictionary<Type, Dictionary<object, object>> _instances;
    private static Dictionary<Type, Dictionary<object, object>> Instances
    {
        get
        {
            if(_instances == null) _instances = new Dictionary<Type, Dictionary<object, object>>();
            return _instances;
        }
    }

    public static T GetStat<T>(this object boundObject, bool createAndBindOnDefault = true, bool useActivator = true)
    {
        return GetStat(boundObject, useActivator ? Activator.CreateInstance<T>() : default, createAndBindOnDefault);
    }

    public static T GetAndCreateStat<T>(params object[] boundObjects)
    {
        int bindingHash = 0;
        foreach(object boundObject in boundObjects)
        {
            bindingHash += boundObject.GetHashCode();
        }
        return GetStat<T>(bindingHash, true, true);
    }

    public static T GetStat<T>(this object boundObject, T existingInstance, bool createAndBindOnDefault = false)
    {
        Type type = existingInstance.GetType(); //typeof(T);
        if (!Instances.ContainsKey(type))
        {
            if (createAndBindOnDefault)
            {
                Instances.Add(type, new Dictionary<object, object>());
                Instances[type].Add(boundObject, existingInstance);
                return (T)Instances[type][boundObject];
            }
            else return existingInstance;
        }
        else if (Instances[type].TryGetValue(boundObject, out var instance))
        {
            return (T)instance;
        }
        else if (createAndBindOnDefault)
        {
            Instances[type].Add(boundObject, existingInstance);
            return (T)Instances[type][boundObject];
        }
        else return existingInstance;
    }

    public static void AddStat(this object boundObject, object instance)
    {
        Type type = instance.GetType();
        if (!Instances.ContainsKey(type))
        {
            Instances.Add(type, new Dictionary<object, object>());
        }
        Instances[type].Add(boundObject, instance);
    }

    public static void RemoveStat(this object boundObject, object instance)
    {
        if (!Instances.ContainsKey(instance.GetType()) || Instances[instance.GetType()].ContainsKey(boundObject)) return;
        Instances[instance.GetType()].Remove(boundObject);
    }

    public static void RemoveAllStats(this object boundObject)
    {
        foreach (var dictionary in Instances)
        {
            dictionary.Value.Remove(boundObject);
        }
    }

    /// <summary>
    /// Clears ALL stats stored (used for cleanup)
    /// </summary>
    public static void ClearAllStats()
    {
        foreach (var dictionary in Instances)
        {
            dictionary.Value.Clear();
        }
        Instances.Clear();
        _instances = null;
    }
}

public interface IBoundInstance : IDisposable
{
    new void Dispose()
    {
        this.RemoveAllStats();
    }
}