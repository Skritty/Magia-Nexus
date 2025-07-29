using System;
using System.Collections.Generic;

public interface IBoundInstances<BindingObjectType, InstanceType>
{
    private static Dictionary<BindingObjectType, InstanceType> instances = new Dictionary<BindingObjectType, InstanceType>();

    public static void AddInstance(InstanceType instance, BindingObjectType owner)
    {
        if (!instances.TryAdd(owner, instance))
        {
            instances[owner] = instance;
        }
    }

    public static void RemoveInstance(BindingObjectType owner)
    {
        if (instances.ContainsKey(owner))
        {
            instances.Remove(owner);
        }
    }

    public static InstanceType GetOrCreateInstance(BindingObjectType owner)
    {
        if (!instances.ContainsKey(owner))
        {
            AddInstance(Activator.CreateInstance<InstanceType>(), owner);
        }
        return instances[owner];
    }

    public static InstanceType GetInstance(BindingObjectType owner, bool createTemporary = true)
    {
        if (!instances.ContainsKey(owner))
        {
            if (createTemporary)
            {
                return Activator.CreateInstance<InstanceType>();
            }
            else return default;
        }
        return instances[owner];
    }
}