public class Effect_Accelerate : EffectTask
{
    public int ticks;
    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        for(int i = 0; i < ticks * multiplier; i++)
        {
            Target.GetMechanic<Mechanic_Actions>().Tick();
        }
    }
}
