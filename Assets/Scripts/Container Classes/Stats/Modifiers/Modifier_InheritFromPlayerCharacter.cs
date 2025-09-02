using System.Collections.Generic;
using UnityEngine;

public class Modifier_InheritFromPlayerCharacter<T> : Solver<T>
{
    public Entity self;
    [SerializeReference]
    public IStat<T> stat;
    public override T Value
    {
        get
        {
            if (self == null || self.GetInstanceID() < 0) return _value;
            Entity owner = self.Stat<Stat_PlayerCharacter>().Value;
            if (owner == null) return _value;
            List<IDataContainer<T>> playerModifiers = (owner.Stat(stat) as Stat<T>).Modifiers;
            if (Modifiers.Count != playerModifiers.Count) // TODO: hehe
            {
                Modifiers.Clear();
                Modifiers.AddRange(playerModifiers);
                Solve();
            }
            return _value;
        }
    }
}
