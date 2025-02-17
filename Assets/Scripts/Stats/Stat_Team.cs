using System.Collections.Generic;
using Sirenix.OdinInspector;

public class Stat_Team : GenericStat<Stat_Team>
{
    [FoldoutGroup("Team")]
    public int team;
    public List<Entity> playerCharacters = new List<Entity>();
    public List<Entity> summons = new List<Entity>();
    public List<Entity> proxies = new List<Entity>();
}