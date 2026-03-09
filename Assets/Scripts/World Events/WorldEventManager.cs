using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;

public class WorldEventManager : Singleton<WorldEventManager>
{
    private HashSet<WorldEvent> resolvedWorldEvents;
}
