using Sirenix.OdinInspector;
using UnityEngine;

public class DamageModifier : DamageSolver, IModifier<float>
{
    public EffectTask Source { get; set; }
    [field: SerializeReference, FoldoutGroup("Modifier")]
    public IStatTag Tag { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public Alignment Alignment { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int MaxStacks { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int StacksAdded { get; private set; } = 1;
    [field: SerializeField, FoldoutGroup("Modifier")]
    public bool PerPlayer { get; private set; }
    [field: SerializeField, FoldoutGroup("Modifier")]
    public int TickDuration { get; private set; }
    [field: ShowInInspector, FoldoutGroup("Modifier"), ReadOnly]
    public bool RefreshDuration { get; private set; }

    public DamageModifier() { }

    public DamageModifier(DamageType damageType, float baseValue)
    {
        this.damageType = damageType;
        _value = baseValue;
    }
}
