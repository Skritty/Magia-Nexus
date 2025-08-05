using UnityEngine;

[CreateAssetMenu(menuName = "ViewableGameAsset/Personality")]
public class Personality : ViewableGameAsset
{
    [SerializeReference]
    public Targeting targeting;
    [SerializeReference]
    public Targeting movement;
    [SerializeReference]
    public MovementDirectionSelector movementSelector;

    public void SetPersonality(Entity entity)
    {
        entity.Stat<Stat_TargetingMethod>().Add(targeting);
        entity.Stat<Stat_MovementTargetingMethod>().Add(movement);
        entity.Stat<Stat_MovementSelector>().Add(movementSelector);
        // TODO: conditional targeting
    }
}
