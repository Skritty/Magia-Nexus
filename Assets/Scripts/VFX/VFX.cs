using Skritty.Tools.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX : PooledObject
{
    public int tickDuration;
    private Transform attachedTo;
    public T PlayVFX<T>(Transform owner, Vector3 offset, Vector3 facing, bool attachToOwner) where T : VFX
    {
        VFX vfx = RequestObject<VFX>();
        vfx.transform.position = owner.position + offset;
        vfx.transform.rotation = Quaternion.FromToRotation(Vector3.up, facing);
        if (attachToOwner) vfx.attachedTo = owner;
        vfx.gameObject.SetActive(true);
        vfx.StartCoroutine(vfx.Effect());
        return (T)vfx;
    }

    private IEnumerator Effect()
    {
        OnStart();
        for (int i = 0; i < tickDuration; i++)
        {
            OnTick();
            yield return new WaitForFixedUpdate();
        }
        OnEnd();
        ReleaseObject();
    }

    protected virtual void OnStart() { }
    protected virtual void OnTick() { }
    protected virtual void OnEnd() { }
}
