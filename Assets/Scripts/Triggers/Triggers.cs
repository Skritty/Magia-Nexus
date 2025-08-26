
// IMPORTANT NOTE: If you want a trigger to work with Effect_AddTrigger, it MUST pass in "Entity owner" as a binding object

using System;

public class Trigger_MovementDirectionCalc : Trigger<Trigger_MovementDirectionCalc, Entity> { }
[Serializable] public class Trigger_PreHit : Trigger<Trigger_PreHit, Hit> { }
[Serializable] public class Trigger_PostHit : Trigger<Trigger_PostHit, Hit> { }
public class Trigger_Damage : Trigger<Trigger_Damage, DamageInstance> { }
public class Trigger_Die : Trigger<Trigger_Die, DamageInstance> { }
public class Trigger_ProjectilePierce : Trigger<Trigger_ProjectilePierce, Entity> { }
public class Trigger_ModifierGained : Trigger<Trigger_ModifierGained, IModifier> { }
public class Trigger_ModifierLost : Trigger<Trigger_ModifierLost, IModifier> { }
public class Trigger_BuffGained : Trigger<Trigger_BuffGained, IModifier> { }
public class Trigger_BuffLost : Trigger<Trigger_BuffLost, IModifier> { }
public class Trigger_DebuffGained : Trigger<Trigger_DebuffGained, IModifier>//, IDataContainer<IModifier>, IDataContainer<int> OR IDataContainer_Counter(<int>)
{
    /*IModifier _modifier;
    IModifier IDataContainer<IModifier>.Value => _modifier;
    int _i;
    int IDataContainer<int>.Value => _i;

    public bool Get<Type>(out Type data)
    {
        IDataContainer<Type> dataContainer = (IDataContainer<Type>)this;
        if (dataContainer != null)
        {
            data = dataContainer.Value;
            return true;
        }
        data = default;
        return false;
    }

    public Trigger_DebuffGained(IModifier modifier = null, int i = 0, params object[] bindingObjects) 
    {
        _modifier = modifier;
        _i = i;
        Invoke(null, bindingObjects);
    }

    public bool IsDefaultValue()
    {
        new Trigger_DebuffGained(null, 19);
        Trigger_DebuffGained.Invoke(null);
        return new Trigger_DebuffGained().Get(out IModifier data);
    }*/
}
public class Trigger_DebuffLost : Trigger<Trigger_DebuffLost, IModifier> { }
public class Trigger_GameEnd : Trigger<Trigger_GameEnd, Viewer> { }
public class Trigger_RoundEnd : Trigger<Trigger_RoundEnd, Viewer> { }
public class Trigger_ActionStart : Trigger<Trigger_ActionStart, Action> { }
public class Trigger_ActionEnd : Trigger<Trigger_ActionEnd, Action> { }
public class Trigger_TurnStart : Trigger<Trigger_TurnStart, Entity> { }
public class Trigger_TurnEnd : Trigger<Trigger_TurnEnd, Entity> { }
public class Trigger_Expire : Trigger<Trigger_Expire, Entity> { }
public class Trigger_ProjectileCreated : Trigger<Trigger_ProjectileCreated, Entity> { }
public class Trigger_SummonCreated : Trigger<Trigger_SummonCreated, Entity> { }
public class Trigger_Channel : Trigger<Trigger_Channel, Entity> { }
public class Trigger_SpellCast : Trigger<Trigger_SpellCast, Spell> { }
public class Trigger_SpellStageIncrement : Trigger<Trigger_SpellStageIncrement, Spell> { }
public class Trigger_SpellMaxStage : Trigger<Trigger_SpellMaxStage, Spell> { }
public class Trigger_SpellEffectApplied : Trigger<Trigger_SpellEffectApplied, Effect> { }
public class Trigger_SpellFinished : Trigger<Trigger_SpellFinished, Spell> { }
public class Trigger_RuneGained : Trigger<Trigger_RuneGained, Rune> { }