using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;
using Skritty.Tools.Utilities;

public class Entity : MonoBehaviour
{
    [SerializeReference, HideReferenceObjectPicker, ListDrawerSettings(ShowFoldout = false, HideRemoveButton = true)]
    private List<BaseStat> baseStats = new List<BaseStat>();
    public T Stat<T>() where T : BaseStat => IBoundInstances<Entity, T>.GetInstance(this);
    public System.Action<Entity> CleanupCallback;

    private void Awake()
    {
        Initialize();
    }

    private void FixedUpdate()
    {
        HandleStats();
    }

    private void Initialize()
    {
        foreach (BaseStat stat in baseStats)
        {
            stat.SetInstanceOwner(this);
        }
        this.Subscribe<Trigger_OnDie>((x) => Debug.Log($"{x.Data<DamageInstance>().target} DIED"));
    }

    private void HandleStats()
    {
        foreach (BaseStat stat in baseStats)
        {
            stat.Tick();
        }
    }

    private void OnDestroy()
    {
        CleanupCallback?.Invoke(this);
    }

    private void OnValidate()
    {
        foreach (BaseStat stat in baseStats)
        {
            stat.owner = this;
        }
    }
}