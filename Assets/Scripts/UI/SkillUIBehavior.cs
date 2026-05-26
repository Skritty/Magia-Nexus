using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;

public class SkillUIBehavior : MonoBehaviour, IDropEvent
{
    public void OnDrop(Transform other)
    {
        Skill skill = null;
        SkillCondition condition = null;
        ViewableGameAssetUIItem VGAUI = null;
        if (transform.TryGetComponent(out VGAUI))
        {
            if (VGAUI.asset is Skill) skill = (Skill)VGAUI.asset;
            else if (VGAUI.asset is SkillCondition) condition = (SkillCondition)VGAUI.asset;
        }
        if (other.TryGetComponent(out VGAUI))
        {
            if (VGAUI.asset is Skill) skill = (Skill)VGAUI.asset;
            else if (VGAUI.asset is SkillCondition) condition = (SkillCondition)VGAUI.asset;
        }

        if (skill != null && condition != null)
        {
            CharacterUIManager.Instance.BindSkill(skill, condition);
        }
        else if (skill != null && other.TryGetComponent(out VGAUI) && VGAUI.asset is Skill)
        {
            CharacterUIManager.Instance.ReorderSkills();
        }
        else if (condition != null && other.TryGetComponent(out VGAUI) && VGAUI.asset is SkillCondition)
        {
            CharacterUIManager.Instance.ReorderSkillConditions();
        }
    }
}
