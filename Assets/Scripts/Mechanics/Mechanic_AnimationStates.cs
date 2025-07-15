using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor.Animations;
using UnityEngine;

public class Mechanic_AnimationStates : Mechanic<Mechanic_AnimationStates>
{
    [FoldoutGroup("Animation States")]
    public Animator animator;
    [FoldoutGroup("Animation States")]
    public AnimatorOverrideController animationSkin;
    private AnimationState _animationState;

    protected override void Initialize()
    {
        base.Initialize();
        animator.runtimeAnimatorController = animationSkin;
    }

    public AnimationState AnimationState
    {
        get => _animationState;
        set
        {
            _animationState = value;
            if(value != AnimationState.None)
                animator.SetTrigger(value.ToString());
        }
    }
}

public enum AnimationState
{
    None,
    Move,
    Stunned,
    AttackWindup,
    AttackFinish,
    CastingWindup,
    CastingFinish
}
