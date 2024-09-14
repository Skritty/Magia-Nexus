using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public interface IBoundInstances<BindingObjectType, InstanceType>
{
    private static Dictionary<BindingObjectType, InstanceType> instances = new Dictionary<BindingObjectType, InstanceType>();
    public static void SetInstanceOwner(InstanceType instance, BindingObjectType owner, Action<BindingObjectType> cleanupCallback)
    {
        if (instances.ContainsKey(owner))
        {
            instances[owner] = instance;
        }
        else
        {
            instances.Add(owner, instance);
            cleanupCallback += (owner) => instances.Remove(owner);
        }
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
    public static InstanceType GetOrCreateInstance(BindingObjectType owner, Action<BindingObjectType> CleanupCallback)
    {
        if (!instances.ContainsKey(owner))
        {
            SetInstanceOwner(Activator.CreateInstance<InstanceType>(), owner, CleanupCallback);
        }   
        return instances[owner];
    }
}

[Serializable]
public abstract class BaseStat
{
    [HideInInspector]
    public Entity owner;
    [HideInInspector]
    public bool baseStat;
    public abstract void SetInstanceOwner(Entity owner);
    protected virtual void Initialize() { }
    public virtual void Tick() { }
}

public abstract class GenericStat<T> : BaseStat, IBoundInstances<Entity, T> where T : GenericStat<T>
{
    public override void SetInstanceOwner(Entity owner)
    {
        this.owner = owner;
        baseStat = true;
        IBoundInstances<Entity, T>.SetInstanceOwner((T)this, owner, owner.CleanupCallback);
        Initialize();
    }
}

public class Stat_Exists : GenericStat<Stat_Exists>
{
    public Action<Entity> OnExpire;
}

public class Stat_PlayerCharacter : GenericStat<Stat_PlayerCharacter>
{

    [FoldoutGroup("Player Character")]
    public TMPro.TextMeshProUGUI characterNamePlate;

    [ShowInInspector, ReadOnly, FoldoutGroup("Player Character")]
    private Viewer player;
    public Viewer Player
    {
        get => player;
        set
        {
            player = value;
            owner.name = value.viewerName;
            if (characterNamePlate != null)
                characterNamePlate.text = value.viewerName;
        }
    }
}

public class Stat_Movement : GenericStat<Stat_Movement>
{
    [FoldoutGroup("Movement")]
    public float movementSpeed;
    [FoldoutGroup("Movement")]
    public Vector3 facingDir = Vector3.right;
}

public class Stat_Target : GenericStat<Stat_Target>
{
    [FoldoutGroup("Target")]
    public Targeting targetingType = new Targeting_Distance();
    [FoldoutGroup("Target")]
    public Entity target;
}

public class Stat_Team : GenericStat<Stat_Team>
{
    [FoldoutGroup("Team")]
    public int team;
}

public class Stat_Untargetable : GenericStat<Stat_Untargetable>
{
    [FoldoutGroup("Untargetable")]
    public bool untargetable;
}

public class Stat_Effect : GenericStat<Stat_Effect>
{
    [FoldoutGroup("Effect")]
    public float effectMultiplier = 1;
    [FoldoutGroup("Effect")]
    public float aoeMultiplier = 1;
}

public class Stat_Ignored : GenericStat<Stat_Ignored>
{
    [ShowInInspector]
    private Dictionary<object, List<Entity>> ignored = new Dictionary<object, List<Entity>>();
    public void AddIgnored(object source, Entity target)
    {
        if (!ignored.ContainsKey(source))
        {
            ignored.Add(source, new List<Entity>());
        }
        ignored[source].Add(target);
    }

    public void RemoveIgnored(object source, Entity target)
    {
        if(ignored.ContainsKey(source))
            ignored[source].Remove(target);
    }

    public bool IsIgnored(object source, Entity target)
    {
        if (ignored.ContainsKey(source) && ignored[source].Contains(target))
            return true;
        return false;
    }
}