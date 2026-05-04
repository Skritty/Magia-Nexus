using Sirenix.OdinInspector;
using UnityEngine;

public class Effect_RemoveModifier : EffectTask
{
    [SerializeReference]
    public Modifier modifier;
    public int stacksRemoved = 1;
    [ShowIf("@modifier == null")]
    public Alignment alignmentRemoved;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        for(int i = 0; i < stacksRemoved; i++)
        {
            if(modifier == null)
            {
                //Target.RemoveOldestDurationModifier(alignmentRemoved); TODO
            }
            else
            {

                modifier.RemoveFromStatTag(Target);
            }
        }
    }
}
