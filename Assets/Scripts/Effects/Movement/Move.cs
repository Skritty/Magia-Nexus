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
            Owner.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Owner.Stat<Stat_Movement>().facingDir);
        }

        Owner.transform.position +=
            Owner.Stat<Stat_Movement>().facingDir
            * movementSpeedMultiplier
            * Mathf.Clamp(Owner.Stat<Stat_Movement>().movementSpeed, 0, 10)
            * Time.fixedDeltaTime;
    }
}
