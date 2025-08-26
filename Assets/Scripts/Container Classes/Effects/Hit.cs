using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[Serializable]
public class Hit : Effect
{
    [FoldoutGroup("@GetType()")]
    public bool triggerPlayerCharacter;
    [FoldoutGroup("@GetType()")]
    public Trigger_PreHit preHit = new Trigger_PreHit();
    [FoldoutGroup("@GetType()")]
    public Trigger_PostHit postHit = new Trigger_PostHit();
    public ITask<Hit>[] preOnHitEffects;
    public ITask<Hit>[] postOnHitEffects;
    [FoldoutGroup("@GetType()")]
    public List<Rune> runes = new();

    public void PreHitTriggers()
    {
        Entity triggerOwner = triggerPlayerCharacter ? Owner.GetMechanic<Mechanic_PlayerOwner>().proxyOwner : Owner;
        System.Action cleanup = preHit.SubscribeToTasks(triggerOwner, 0, 0, false, preOnHitEffects);
        Trigger_PreHit.Invoke(this, this, triggerOwner, Target);
        cleanup.Invoke();
    }

    public void PostHitTriggers()
    {
        Entity triggerOwner = triggerPlayerCharacter ? Owner.GetMechanic<Mechanic_PlayerOwner>().proxyOwner : Owner;
        System.Action cleanup = postHit.SubscribeToTasks(triggerOwner, 0, 0, false, postOnHitEffects);
        Trigger_PostHit.Invoke(this, this, triggerOwner, Target);
        cleanup.Invoke();
    }

    public Hit Clone()
    {
        Hit clone = (Hit)MemberwiseClone();
        clone.runes = new List<Rune>(runes);
        return clone;
    }
}