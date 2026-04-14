using System.Collections;
using System.Collections.Generic;
using Skritty.Tools.Utilities;
using UnityEngine;

public class PersistantDataStoreManager : Singleton<PersistantDataStoreManager>
{
    public List<PersistantDataStore> stores = new List<PersistantDataStore>();
    [HideInInspector]
    public bool taken;
}
