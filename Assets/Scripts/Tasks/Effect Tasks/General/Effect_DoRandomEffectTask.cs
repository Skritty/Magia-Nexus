using System.Collections.Generic;
using Sirenix.OdinInspector;
using Skritty.Tools.Utilities;
using UnityEngine;

[LabelText("Task: Do Random Effect")]
public class Effect_DoRandomEffectTask<T> : EffectTask
{
    public EffectTargetSelector proxy;
    public bool useProxyAsOwner;
    [SerializeReference]
    public List<EffectTask> effects;
    public List<Action> actions; // TODO: actions should NOT be a source of truth

    public override void DoEffect(Entity owner, Entity target, float multiplier, bool triggered)
    {
        WeightedChance<EffectTask> random = new WeightedChance<EffectTask>();
        if (actions.Count > 0)
        {
            foreach (Action action in actions)
            {
                random.Add(action.effects[0], 1);
            }
        }
        else
        {
            foreach (EffectTask e in effects)
            {
                random.Add(e, 1);
            }
        }

        if (proxy != EffectTargetSelector.None)
        {
            Entity proxyEntity = proxy == EffectTargetSelector.Owner ? owner : target;
            random.GetRandomEntry().DoTask(owner, target, proxyEntity);
        }
        else
        {
            random.GetRandomEntry().DoTask(owner, target, null);
        }
    }
}