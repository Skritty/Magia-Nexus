using System;
using UnityEngine;

public class Stat_Equipment : PrioritySolver<Item> { }
public class Stat_Equipment_Head : Stat_Equipment, IStat { }
public class Stat_Equipment_Body : Stat_Equipment, IStat { }
public class Stat_Equipment_Legs : Stat_Equipment, IStat { }
public class Stat_Equipment_Arms : Stat_Equipment, IStat { }
public class Stat_Equipment_Feet : Stat_Equipment, IStat { }
public class Stat_Equipment_HandL : Stat_Equipment, IStat { }
public class Stat_Equipment_HandR : Stat_Equipment, IStat { }
public class Stat_Equipment_Back : Stat_Equipment, IStat { }
public class Stat_Equipment_Neck : Stat_Equipment, IStat { }
public class Stat_Equipment_FingerL : Stat_Equipment, IStat { }
public class Stat_Equipment_FingerR : Stat_Equipment, IStat { }
public class Stat_Equipment_WristL : Stat_Equipment, IStat { }
public class Stat_Equipment_WristR : Stat_Equipment, IStat { }
public class Mechanic_Inventory : Mechanic<Mechanic_Inventory>
{
    public override void Initialize()
    {
        Owner.Stat<Stat_Equipment_Body>().Value?.OnGained(Owner);
        Owner.Stat<Stat_Equipment_Back>().Value?.OnGained(Owner);
    }
}
