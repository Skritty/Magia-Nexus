public class Effect_RemovePersistantEffect : EffectTask
{
    public int stacksRemoved = 1;
    public Alignment alignmentRemoved;

    public override void DoEffect(Entity Owner, Entity Target, float multiplier, bool triggered)
    {
        Target.RemoveModifier(alignmentRemoved, stacksRemoved);
    }
}
