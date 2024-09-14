using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : Effect
{
    public float movementSpeedMultiplier = 1f;
    public bool rotate;

    public override void Activate()
    {
        if (rotate)
        {
            owner.transform.forward = owner.Stat<Stat_Movement>().facingDir;
        }

        owner.transform.position +=
            owner.Stat<Stat_Movement>().facingDir
            * movementSpeedMultiplier
            * Mathf.Clamp(owner.Stat<Stat_Movement>().movementSpeed, 0, 10)
            * Time.fixedDeltaTime;
    }
}
