using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CharacterUIManager : Singleton<CharacterUIManager>, IPointerDownHandler, IUpdatable
{
    public Entity selectedCharacter;
    public ViewableGameAssetUIItem defaultVGAUI;
    public RectTransform skills, skillConditions;
    public Sprite lineImage;
    public Vector2 graphScale;
    public float lineWidth;
    private Entity previouslySelected;
    public void OnPointerDown(PointerEventData eventData)
    {
        // Add character list/clicking on characters in the scene (character select)
    }

    private void Update()
    {
        if(previouslySelected != selectedCharacter)
        {
            previouslySelected = selectedCharacter;
            UpdateUI();
        }
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
        foreach (Action skill in selectedCharacter.Stat<Stat_Skills>())
        {
            ViewableGameAssetUIItem VGAUI = defaultVGAUI.RequestObject<ViewableGameAssetUIItem>();
            VGAUI.transform.parent = skills;
            VGAUI.asset = skill;
            VGAUI.UpdateUI();
        }

        foreach (Transform conditionUI in skillConditions)
        {
            if (conditionUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                VGAUI.ReleaseObject();
            }
        }
        foreach (SkillCondition condition in selectedCharacter.Stat<Stat_SkillConditions>())
        {
            ViewableGameAssetUIItem VGAUI = defaultVGAUI.RequestObject<ViewableGameAssetUIItem>();
            VGAUI.transform.parent = skillConditions;
            VGAUI.asset = condition;
            VGAUI.UpdateUI();
        }
        UpdateSkillConnections();
    }

    private void UpdateSkillConnections()
    {
        foreach(var pair in selectedCharacter.GetMechanic<Mechanic_Skills>().conditionBindings)
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
        selectedCharacter.GetMechanic<Mechanic_Skills>().BindAction(skill, condition);
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
        selectedCharacter.Stat<Stat_Skills>().Clear();
        selectedCharacter.Stat<Stat_Skills>().AddRange(reordered);
        UpdateSkillConnections();
    }

    public void ReorderSkillConditions()
    {
        List<SkillCondition> reordered = new();
        foreach (Transform conditionUI in skillConditions)
        {
            if (conditionUI.TryGetComponent(out ViewableGameAssetUIItem VGAUI))
            {
                reordered.Add(VGAUI.asset as SkillCondition);
            }
        }
        selectedCharacter.Stat<Stat_SkillConditions>().Clear();
        selectedCharacter.Stat<Stat_SkillConditions>().AddRange(reordered);
        UpdateSkillConnections();
    }
}
