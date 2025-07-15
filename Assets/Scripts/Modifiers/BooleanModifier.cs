using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class BooleanModifier : BooleanSolver, IModifier<bool>
{
    public BooleanModifier() { }
    public EffectTask Source { get; set; }
    [field: SerializeReference]
    public IStatTag Tag { get; set; }
    public int MaxStacks { get; set; } = -1;
    public int Stacks { get; set; } = 1;
    public bool PerPlayer { get; set; }
    public Alignment Alignment { get; set; }
    public bool Temporary { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int Tick { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int TickDuration { get; set; } = -1;
    [field: ShowIf("@Temporary"), ReadOnly]
    public bool RefreshDuration { get; set; }
}