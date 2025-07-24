public class Effect_Accelerate : Effect_Accelerate<Effect> { }
public class Effect_Accelerate<T> : EffectTask<T>
{
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        for(int i = 0; i < multiplier; i++)
        {
            Target.GetMechanic<Mechanic_Actions>().Tick();
        }
    }
}
