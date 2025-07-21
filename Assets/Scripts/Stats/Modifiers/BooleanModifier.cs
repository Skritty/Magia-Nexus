using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BooleanModifier : BooleanPrioritySolver, IModifier<bool>
{
    public BooleanModifier() { }
    public EffectTask Source { get; set; }
    [field: SerializeReference]
    public IStatTag Tag { get; }
    public Alignment Alignment { get; }
    public int MaxStacks { get; }
    public int StacksAdded { get; } = 1;
    public bool PerPlayer { get; }
    public bool Temporary { get; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int TickDuration { get; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public bool RefreshDuration { get; }
}