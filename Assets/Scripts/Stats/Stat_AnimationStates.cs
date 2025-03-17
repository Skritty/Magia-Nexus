using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor.Animations;
using UnityEngine;

public class Stat_AnimationStates : GenericStat<Stat_AnimationStates>
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
            animator.SetTrigger(value.ToString());
        }
    }
}

public enum AnimationState
{
    Move,
    Stunned,
    AttackWindup,
    AttackFinish,
    CastingWindup,
    CastingFinish
}
