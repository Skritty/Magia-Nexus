using System.Collections;
using System.Collections.Generic;
using TwitchLib.Api.Helix;
using UnityEngine;

public class Targeting_Line : MultiTargeting
{
    public Targeting_Line() { }
    public Targeting_Line(float length, float width)
    {
        this.length = length;
        this.width = width;
    }
    public float length, width;
    public bool faceFirstTarget;
    private Vector3 dirToTarget;

    protected override bool IsValidTarget(Entity owner, Entity proxy, Entity target, bool firstTarget)
    {
        if (length == 0 || width == 0) return false;

        Vector3 dirToEntity = target.transform.position - GetCenter(owner, proxy);
        dirToTarget = firstTarget && faceFirstTarget ? dirToEntity : owner.GetMechanic<Mechanic_Movement>().facingDir;
        Vector3 projectedDir = Vector3.Project(dirToEntity, dirToTarget);
        if (Vector3.Dot(dirToEntity, dirToTarget) < 0) return false;
        if (projectedDir.magnitude > length * owner.Stat<Stat_AoESize>().Value) return false;
        if ((dirToEntity - projectedDir).magnitude > width * owner.Stat<Stat_AoESize>().Value) return false;
        return true;
    }

    protected override void DoFX(Entity owner, Entity proxy, List<Entity> targets)
    {
        if (vfx is not VFX_Line) return;
        VFX_Line line = vfx.PlayVFX<VFX_Line>((proxy != null ? proxy : owner).transform, offset, Vector3.up, true);
        line.ApplyLine(length, width);
        //line.ApplyDamage(source as Effect_Damage<Effect>);
    }

    public override void OnDrawGizmos(Transform source)
    {
        if (source == null) return;
        Gizmos.color = new Color(.8f, .2f, .2f, .8f);
        Gizmos.DrawCube(source.position + (Vector3.up * length)/2, new Vector3(width, length, 0));
    }
}
