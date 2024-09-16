using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Targeting_Radial : MultiTargeting
{
    public Targeting_Radial() { }
    public Targeting_Radial(float radius, float angle) 
    {
        this.radius = radius;
        this.angle = angle;
    }
    public float radius;
    [Range(0, 180)]
    public float angle = 180;

    protected override bool IsValidTarget(Entity target)
    {
        Vector3 dirToEntity = target.transform.position - owner.transform.position;
        if (dirToEntity.magnitude > radius * owner.Stat<Stat_EffectModifiers>().aoeMultiplier) return false;
        if (angle >= 180) return true;

        Vector3 dirToTarget = owner.Stat<Stat_Movement>().facingDir;
        if (Mathf.Acos(Vector3.Dot(dirToEntity, dirToTarget) 
            / (dirToEntity.magnitude * dirToTarget.magnitude)) > angle) return false;
        return true;
    }

    public override void OnDrawGizmos(Transform source)
    {
        if (source == null) return;
        Gizmos.color = new Color(.8f,.2f, .2f, .8f);
        Gizmos.DrawSphere(source.position, radius);
        Gizmos.color = new Color(.2f, .8f, .8f, 1f);
        Gizmos.DrawLine(source.position, Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up * radius);
        Gizmos.DrawLine(source.position, Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.up * radius);
    }
}