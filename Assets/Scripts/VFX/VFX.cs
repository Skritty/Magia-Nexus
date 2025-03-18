using Skritty.Tools.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFX : PooledObject
{
    public int tickDuration;
    public VisualEffect visualEffect;
    private Transform attachedTo;
    private Vector3 offset;
    private Coroutine effect;
    public T PlayVFX<T>(Transform owner, Vector3 offset, Vector3 facing, bool attachToOwner) where T : VFX
    {
        this.offset = offset;
        VFX vfx = RequestObject<VFX>();
        vfx.transform.position = owner.position + offset;
        vfx.transform.rotation = Quaternion.FromToRotation(Vector3.up, facing);
        if (attachToOwner) vfx.attachedTo = owner;
        vfx.gameObject.SetActive(true);
        //if (effect != null) vfx.StopCoroutine(effect);
        effect = vfx.StartCoroutine(vfx.Effect());
        return (T)vfx;
    }

    public void StopVFX()
    {
        //StopCoroutine(effect);
        OnEnd();
        ReleaseObject();
        //visualEffect.SendEvent(,);
    }

    private IEnumerator Effect()
    {
        OnStart();
        for (int i = 0; i < tickDuration; i++)
        {
            if(attachedTo != null)
            {
                transform.position = attachedTo.position + offset;
            }
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
