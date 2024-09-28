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

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (BaseStat stat in baseStats)
        {
            stat.AddInstance(this);
        }
    }

    private void FixedUpdate()
    {
        HandleStats();
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
        foreach (BaseStat stat in baseStats)
        {
            stat.OnDestroy();
            stat.RemoveInstance(this);
        }
    }

    private void OnValidate()
    {
        foreach (BaseStat stat in baseStats)
        {
            if (stat == null) continue;
            stat.Owner = this;
        }
    }
}