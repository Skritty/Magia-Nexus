using UnityEngine;

public class Effect
{
    [field: SerializeField]
    public float EffectMultiplier { get; set; }
    public Entity Owner { get; set; }
    public Entity Target { get; set; }
}