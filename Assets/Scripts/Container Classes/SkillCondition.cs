using UnityEngine;

[CreateAssetMenu(menuName = "ViewableGameAsset/SkillCondition")]
public class SkillCondition : ViewableGameAsset
{
    [SerializeReference]
    public Trigger condition;
}
