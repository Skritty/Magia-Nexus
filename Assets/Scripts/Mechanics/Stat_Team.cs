using System.Collections.Generic;
using Sirenix.OdinInspector;

public class Stat_Team : Mechanic<Stat_Team>
{
    [FoldoutGroup("Team")]
    public int team;
    [FoldoutGroup("Team")]
    public List<Entity> playerCharacters = new List<Entity>();
    [FoldoutGroup("Team")]
    public List<Entity> summons = new List<Entity>();
    [FoldoutGroup("Team")]
    public List<Entity> proxies = new List<Entity>();
}