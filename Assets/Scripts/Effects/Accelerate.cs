using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accelerate : Effect
{
    public override void Activate()
    {
        for(int i = 0; i < effectMultiplier; i++)
        {
            Target.Stat<Stat_Actions>().Tick();
        }
    }
}
