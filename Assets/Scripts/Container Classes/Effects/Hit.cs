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
        System.Action cleanup = preHit.SubscribeToTasks(Owner, 0, 0, false, preOnHitEffects);
        Trigger_PreHit.Invoke(this, this, Owner, Target);
        cleanup.Invoke();
    }

    public void PostHitTriggers()
    {
        System.Action cleanup = postHit.SubscribeToTasks(Owner, 0, 0, false, postOnHitEffects);
        Trigger_PostHit.Invoke(this, this, Owner, Target);
        cleanup.Invoke();
    }

    public Hit Clone()
    {
        Hit clone = (Hit)MemberwiseClone();
        clone.runes = new List<Rune>(runes);
        return clone;
    }
}