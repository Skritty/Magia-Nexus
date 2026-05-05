using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterUIManager : Singleton<CharacterUIManager>, IPointerDownHandler, IUpdatable
{
    public Entity selectedCharacter;
    public ViewableGameAssetUIItem defaultVGAUI;
    public RectTransform skills, conditions, reactions;
    public Sprite lineImage;
    public Vector2 graphScale;
    public float lineWidth;
    public List<Entity> partyMembers;
    public void OnPointerDown(PointerEventData eventData)
    {
        // Add character list/clicking on characters in the scene (character select)
    }

    public void UpdateUI()
    {
        UpdateSkillUI();
    }

    private void UpdateSkillUI()
    {
        foreach (Transform skillUI in skills)
        {
            if (skillUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                VGAUI.ReleaseObject();
            }
        }
        foreach (Action skill in selectedCharacter.GetStat<Stat_Skills>())
        {
            ViewableGameAssetUIItem VGAUI = defaultVGAUI.RequestObject<ViewableGameAssetUIItem>();
            VGAUI.transform.SetParent(skills, true);
            VGAUI.asset = skill;
            VGAUI.UpdateUI();
        }

        foreach (Transform conditionUI in conditions)
        {
            if (conditionUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                VGAUI.ReleaseObject();
            }
        }
        foreach (SkillCondition condition in selectedCharacter.GetStat<Stat_SkillConditions>())
        {
            ViewableGameAssetUIItem VGAUI = defaultVGAUI.RequestObject<ViewableGameAssetUIItem>();
            VGAUI.transform.SetParent(conditions, true);
            VGAUI.asset = condition;
            VGAUI.UpdateUI();
        }

        foreach (Transform reactionUI in reactions)
        {
            if (reactionUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                VGAUI.ReleaseObject();
            }
        }
        foreach (SkillCondition reaction in selectedCharacter.GetStat<Stat_SkillReactions>())
        {
            ViewableGameAssetUIItem VGAUI = defaultVGAUI.RequestObject<ViewableGameAssetUIItem>();
            VGAUI.transform.SetParent(reactions, true);
            VGAUI.asset = reaction;
            VGAUI.UpdateUI();
        }
        UpdateSkillConnections();
    }

    private void UpdateSkillConnections()
    {
        foreach(var pair in selectedCharacter.GetStat<Mechanic_Skills>().conditionBindings)
        {
            
        }
    }

    private void CreateLineSprite(float ax, float ay, float bx, float by, Color col)
    {
        GameObject NewObj = new GameObject();
        NewObj.name = "line from " + ax + " to " + bx;
        Image NewImage = NewObj.AddComponent<Image>();
        NewImage.sprite = lineImage;
        NewImage.color = col;
        RectTransform rect = NewObj.GetComponent<RectTransform>();
        rect.SetParent(transform);
        rect.localScale = Vector3.one;

        Vector3 a = new Vector3(ax * graphScale.x, ay * graphScale.y, 0);
        Vector3 b = new Vector3(bx * graphScale.x, by * graphScale.y, 0);


        rect.localPosition = (a + b) / 2;
        Vector3 dif = a - b;
        rect.sizeDelta = new Vector3(dif.magnitude, lineWidth);
        rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
    }

    public void BindSkill(Action skill, SkillCondition condition)
    {
        selectedCharacter.GetStat<Mechanic_Skills>().BindAction(skill, condition);
        Debug.Log($"Bound {skill.name} to {condition.name}");
    }

    public void ReorderSkills()
    {
        List<Action> reordered = new();
        foreach (Transform skillUI in skills)
        {
            if (skillUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                reordered.Add(VGAUI.asset as Action);
            }
        }
        selectedCharacter.GetStat<Stat_Skills>().Clear();
        selectedCharacter.GetStat<Stat_Skills>().AddRange(reordered);
        UpdateSkillConnections();
    }

    public void ReorderSkillConditions()
    {
        List<SkillCondition> reordered = new();
        foreach (Transform conditionUI in conditions)
        {
            if (conditionUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                reordered.Add(VGAUI.asset as SkillCondition);
            }
        }
        selectedCharacter.GetStat<Stat_SkillConditions>().Clear();
        selectedCharacter.GetStat<Stat_SkillConditions>().AddRange(reordered);
        UpdateSkillConnections();
    }

    public void SelectPartyMember(int number)
    {
        if (partyMembers.Count <= number) return;
        selectedCharacter = partyMembers[number];
        UpdateUI();
    }
}
