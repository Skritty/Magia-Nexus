using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTrigger : Effect
{
    [SerializeReference]
    public Trigger trigger;
    public override void Activate()
    {
        trigger.Invoke();
    }
}
