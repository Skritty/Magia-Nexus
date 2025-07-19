using Sirenix.OdinInspector;
using UnityEngine;

public class DamageModifier : DamageSolver, IModifier<float>
{
    public EffectTask Source { get; set; }
    [field: SerializeReference]
    public IStatTag Tag { get; set; }
    public Alignment Alignment { get; set; }
    public int MaxStacks { get; set; }
    public int StacksAdded { get; set; } = 1;
    public int Stacks { get; set; } = 1;
    public bool PerPlayer { get; set; }
    public bool Temporary { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int CurrentTick { get; set; }
    [field: ShowIf("@Temporary"), ReadOnly]
    public int TickDuration { get; set; } = -1;
    [field: ShowIf("@Temporary"), ReadOnly]
    public bool RefreshDuration { get; set; }

    public DamageModifier() { }

    public DamageModifier(DamageType damageType, float baseValue)
    {
        this.damageType = damageType;
        _value = baseValue;
    }
}
