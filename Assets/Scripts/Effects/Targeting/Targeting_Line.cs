using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Targeting_Line : MultiTargeting
{
    public Targeting_Line() { }
    public Targeting_Line(float length, float width)
    {
        this.length = length;
        this.width = width;
    }
    public float length, width;

    protected override bool IsValidTarget(Entity target)
    {
        if (length == 0 || width == 0) return false;

        Vector3 dirToEntity = target.transform.position - owner.transform.position;
        Vector3 dirToTarget = owner.Stat<Stat_Movement>().facingDir;
        Vector3 projectedDir = Vector3.Project(dirToEntity, dirToTarget);
        if (Vector3.Dot(dirToEntity, dirToTarget) < 0) return false;
        if (projectedDir.magnitude > length * owner.Stat<Stat_EffectModifiers>().aoeMultiplier) return false;
        if ((dirToEntity - projectedDir).magnitude > width * owner.Stat<Stat_EffectModifiers>().aoeMultiplier) return false;
        return true;
    }

    public override void OnDrawGizmos(Transform source)
    {
        if (source == null) return;
        Gizmos.color = new Color(.8f, .2f, .2f, .8f);
        Gizmos.DrawCube(source.position + (Vector3.up * length)/2, new Vector3(width, length, 0));
    }
}
