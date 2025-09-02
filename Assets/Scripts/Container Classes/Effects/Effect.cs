using UnityEngine;

public class Stat_EffectMultiplier : NumericalSolver, IStat<float> { }
public class Effect
{
    [SerializeField]
    protected float _effectMultiplier;
    public float EffectMultiplier 
    {
        get => _effectMultiplier * (Owner == null ? 1 : Owner.Stat<Stat_EffectMultiplier>().Value);
        set => _effectMultiplier = value;
    }
    public Entity Owner { get; set; }
    public Entity Target { get; set; }
}