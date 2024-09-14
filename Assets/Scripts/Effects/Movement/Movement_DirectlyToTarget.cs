using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_DirectlyToTarget : PersistentEffect
{
    public override void Activate()
    {
        SetMoveDir();
    }

    public override void OnAction()
    {
        SetMoveDir();
    }

    private void SetMoveDir()
    {
        target = owner.Stat<Stat_Target>().target;
        if (target == null)
        {
            owner.Stat<Stat_Movement>().facingDir = Vector3.zero;
        }
        else
        {
            owner.Stat<Stat_Movement>().facingDir = (target.transform.position - owner.transform.position).normalized;
        }
    }
}