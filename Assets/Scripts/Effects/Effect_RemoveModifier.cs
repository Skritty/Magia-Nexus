public class Effect_RemoveModifier<T> : EffectTask<T>
{
    public int stacksRemoved = 1;
    public Alignment alignmentRemoved;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        for(int i = 0; i < stacksRemoved; i++)
        {
            Target.RemoveOldestDurationModifier(alignmentRemoved);
        }
    }
}
