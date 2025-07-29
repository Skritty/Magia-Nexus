using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[LabelText("Task: Add Runes to Damage Instance")]
public class Task_ModifyDamageInstanceRunes : ITask<DamageInstance>
{
    [SerializeReference]
    public List<Rune> runes;
    public bool DoTask(DamageInstance damage)
    {
        damage.runes.AddRange(runes);
        return true;
    }
}