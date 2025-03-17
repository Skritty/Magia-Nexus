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

    protected override bool IsValidTarget(Entity target, bool firstTarget)
    {
        Vector3 dirToEntity = target.transform.position - GetCenter();
        if (dirToEntity.magnitude > radius * Owner.Stat<Stat_EffectModifiers>().CalculateModifier(EffectTag.AoESize)) return false;
        if (angle >= 180) return true;

        Vector3 dirToTarget = Owner.Stat<Stat_Movement>().facingDir;
        float fromTo = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(dirToEntity, dirToTarget) / (dirToEntity.magnitude * dirToTarget.magnitude));
        if (float.IsNaN(fromTo)) fromTo = 0;
        if (fromTo > angle) return false;
        return true;
    }

    protected override void DoFX(Effect source, List<Entity> targets)
    {
        if (vfx is not VFX_AoE) return;
        VFX_AoE aoe = vfx.PlayVFX<VFX_AoE>((proxy != null ? proxy : Owner).transform, offset, Vector3.up, true);
        aoe.ApplyAoE(radius, angle);
        aoe.ApplyDamage(source as DamageInstance);
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