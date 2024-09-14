using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PE_MovementSpeedModifer : PersistentEffect
{
    public float speedIncrease;
    public override void OnGained()
    {
        target.Stat<Stat_Movement>().movementSpeed += speedIncrease; 
    }

    public override void OnLost()
    {
        target.Stat<Stat_Movement>().movementSpeed -= speedIncrease;
    }
}